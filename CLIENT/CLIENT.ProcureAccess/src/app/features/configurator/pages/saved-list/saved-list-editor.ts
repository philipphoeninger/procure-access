// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable, signal } from '@angular/core';

import { Criterion, PriorityLevel } from '../../models/criteria';
import { CriterionDecision, CriterionDecisions } from '../../models/criteria-list';
import { SavedListDecisionRequest } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import {
  decisionOf,
  effectiveLevel,
  noteLengthError,
  overridePriority,
  resetPriority,
  setIncluded,
  setNote,
  setWeight,
} from '../../state/criteria-list-rules';
import { ConfiguratorSettings } from '../../state/configurator-settings';

/** A note is saved this long after the last keystroke (owner decision S5-21b 1a). */
export const NOTE_SAVE_DELAY_MS = 800;

/** The body of `PATCH …/criteria/{revision}` for a decision: the weight only while B applies, no empty note. */
export function toDecisionRequest(criterion: Criterion, decision: CriterionDecision): SavedListDecisionRequest {
  return {
    isIncluded: decision.included,
    overrideLevel: decision.overrideLevel,
    overrideWeight: effectiveLevel(criterion, decision) === 'B' ? decision.overrideWeight : null,
    note: decision.note === '' ? null : decision.note,
  };
}

function sameDecision(a: CriterionDecision, b: CriterionDecision): boolean {
  return (
    a.included === b.included &&
    a.overrideLevel === b.overrideLevel &&
    a.overrideWeight === b.overrideWeight &&
    a.note === b.note
  );
}

/**
 * The decisions of a saved list while an Editor or Owner changes them (R-SAV-03; owner decision
 * S5-21b 1a): every change shows at once and is saved right away with
 * `PATCH /configurations/{id}/criteria/{revision}`, the whole decision of that criterion; a note
 * {@link NOTE_SAVE_DELAY_MS} after the last keystroke, and only while it is not too long (the card
 * shows that error). The rules are S11's (`criteria-list-rules`), so both lists decide alike.
 *
 * - The saves of one criterion run one after the other, each with the decision of that moment, so
 *   the server ends with what the screen shows.
 * - A failed save puts the criterion back to its last saved decision, unless the user has changed
 *   it again meanwhile (then the next save carries the newer decision). The error interceptor shows
 *   the general error, or the server page for a 5xx.
 * - {@link flush} saves a waiting note at once; the page calls it when it is left. {@link settled}
 *   also waits for every save, before an export or a copy reads the stored list.
 *
 * One per saved-list route (route-level provider), like `SavedListLoad`.
 */
@Injectable()
export class SavedListEditor {
  private readonly api = inject(ConfiguratorApiService);
  private readonly settings = inject(ConfiguratorSettings);

  private readonly current = signal<CriterionDecisions>({});
  /** The decisions as shown, by criterion key. */
  readonly decisions = this.current.asReadonly();

  private listId = '';
  private criteria = new Map<string, Criterion>();
  /** The last decision the server confirmed, by key: where a failed save goes back to. */
  private saved = new Map<string, CriterionDecision>();
  /** The running saves by key, so one criterion's saves keep their order. */
  private chains = new Map<string, Promise<void>>();
  private noteTimers = new Map<string, ReturnType<typeof setTimeout>>();

  /** Starts on a loaded list; waiting notes of an earlier list are dropped. */
  start(listId: string, criteria: readonly Criterion[], decisions: CriterionDecisions): void {
    this.cancelNoteTimers();
    this.listId = listId;
    this.criteria = new Map(criteria.map((criterion) => [criterion.key, criterion]));
    this.saved = new Map(criteria.map((criterion) => [criterion.key, decisionOf(decisions, criterion.key)]));
    this.chains = new Map();
    this.current.set(decisions);
  }

  setIncluded(key: string, included: boolean): void {
    this.change(key, (decisions) => setIncluded(decisions, key, included));
  }

  overridePriority(key: string, level: PriorityLevel): void {
    this.change(key, (decisions, criterion) => overridePriority(decisions, criterion, level));
  }

  setWeight(key: string, weight: number): void {
    this.change(key, (decisions, criterion) => setWeight(decisions, criterion, weight, this.settings.weightRange()));
  }

  resetPriority(key: string): void {
    this.change(key, (decisions) => resetPriority(decisions, key));
  }

  setNote(key: string, note: string): void {
    const criterion = this.criteria.get(key);
    if (criterion === undefined) return;
    this.current.update((decisions) => setNote(decisions, key, note));
    clearTimeout(this.noteTimers.get(key));
    this.noteTimers.set(
      key,
      setTimeout(() => {
        this.noteTimers.delete(key);
        this.save(key);
      }, NOTE_SAVE_DELAY_MS),
    );
  }

  /** Saves every waiting note now (the page is left). */
  flush(): void {
    for (const key of [...this.noteTimers.keys()]) {
      clearTimeout(this.noteTimers.get(key));
      this.noteTimers.delete(key);
      this.save(key);
    }
  }

  /**
   * Saves every waiting note now and resolves when every save has answered, so an export or a
   * copy, which read the stored list, include the last change.
   */
  async settled(): Promise<void> {
    this.flush();
    await Promise.all([...this.chains.values()]);
  }

  private change(key: string, rule: (decisions: CriterionDecisions, criterion: Criterion) => CriterionDecisions): void {
    const criterion = this.criteria.get(key);
    if (criterion === undefined) return;
    const before = this.current();
    const after = rule(before, criterion);
    if (after === before) return;
    this.current.set(after);
    this.save(key);
  }

  private save(key: string): void {
    const criterion = this.criteria.get(key);
    if (criterion === undefined) return;
    const listId = this.listId;
    const previous = this.chains.get(key) ?? Promise.resolve();
    const next = previous.then(async () => {
      if (listId !== this.listId) return;
      const decision = decisionOf(this.current(), key);
      // A note that is too long is not sent; the card shows its length error (R-LST-05).
      if (noteLengthError(decision.note) !== null) return;
      if (sameDecision(decision, this.saved.get(key) ?? decisionOf({}, key))) return;
      try {
        await this.api.decide(listId, criterion.revisionId, toDecisionRequest(criterion, decision));
        if (listId === this.listId) this.saved.set(key, decision);
      } catch {
        if (listId !== this.listId || !sameDecision(decisionOf(this.current(), key), decision)) return;
        const back = this.saved.get(key) ?? decisionOf({}, key);
        this.current.update((decisions) => ({ ...decisions, [key]: back }));
      }
    });
    this.chains.set(key, next);
  }

  private cancelNoteTimers(): void {
    for (const timer of this.noteTimers.values()) clearTimeout(timer);
    this.noteTimers.clear();
  }
}
