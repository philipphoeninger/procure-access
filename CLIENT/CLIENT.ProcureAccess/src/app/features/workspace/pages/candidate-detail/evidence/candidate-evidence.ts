// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TextFieldModule } from '@angular/cdk/text-field';
import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  DOCUMENT,
  effect,
  ElementRef,
  inject,
  Injector,
  signal,
  untracked,
  viewChild,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { problemDetails, problemFieldErrors, problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ConfirmDialog } from '@app/shared/components/confirm-dialog/confirm-dialog';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { formatGermanDate, todayIsoDate } from '@app/shared/date/german-date';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import { Candidate, EVIDENCE_LIMITS, TEST_PROCEDURES, TestEvidence, TestStatus } from '../../../models/candidates';
import { CandidatesApiService } from '../../../services/api/candidates-api.service';
import { candidateTabTitle } from '../candidate-detail';
import { CandidateLoad, isProjectEditor } from '../candidate-load';
import { EVIDENCE_FIELDS, EVIDENCE_LABELS, EVIDENCE_PATHS, EvidenceCard, EvidenceField, KindChoice } from './evidence-card';

/** The order of the radio group (UX copy §5.8, Handoff §5.1). */
const STATUSES: readonly TestStatus[] = ['tested', 'notTested', 'unknown'];

/**
 * S34a, the tab „Prüfnachweis“ at `/projekte/:id/produkte/:pid/pruefnachweis` (R-CAN-05; Handoff
 * §5.1, Figma `2022:267`, S34a-J `2094:2525`, S34a-L `2144:3508`).
 *
 * - A card with the radio group „Wurde das Produkt auf Barrierefreiheit geprüft?“ (Ja / Nein /
 *   Nicht bekannt, preset by the server's „Nicht bekannt“) and its hint. While it is not „Ja“,
 *   `candidate.evidence.details.hint` below the card; with „Ja“ the H2 „Prüfnachweise“ and the
 *   evidence cards follow in the DOM — no focus jump, no announcement.
 * - Each card: H3 „Prüfnachweis n: Art“ („Prüfnachweis n“ without a kind), then Art der Prüfung
 *   (the five catalog procedures, „Andere Prüfung“ with its hint line, „Eigene Prüfung ohne
 *   festes Verfahren“; „Andere Prüfung“ inserts the required „Name des Prüfverfahrens“ after the
 *   select), Wer hat geprüft?, Datum der Prüfung (TT.MM.JJJJ, not after today; a text field
 *   until the calendar has its texts, owner decision S6-17 D6 (a)), Geprüfte Version, Link zum
 *   Prüfbericht, the private note with „Nur für Ihre Organisation: …“ (PRV-01) and „Prüfnachweis n
 *   entfernen“ (announces `.removed`, focus to the previous card's heading or „Weiteren
 *   Prüfnachweis hinzufügen“). „Weiteren Prüfnachweis hinzufügen“ adds a card and focuses its
 *   „Art der Prüfung“. Choosing „Ja“ without evidences shows one empty card, which „Speichern“
 *   skips while nobody typed into it (D4 (b)): „Ja“ with no evidence is allowed (WSP-01).
 * - Leaving „Ja“ with evidences asks `candidate.evidence.list.statusChange` (D2 (a)); „Abbrechen“
 *   keeps „Ja“, „Nachweise entfernen“ drops the cards (removed on „Speichern“).
 * - One „Speichern“ for the tab, as the frames (D1 (a)): first the test status with the
 *   candidate's tag (leaving „Ja“ removes the removed evidences with it; a 409
 *   `test-status-conflict` asks again with the server's count and repeats with `removeEvidences`),
 *   then the removed evidences with their own tags, then each card that is new (no tag) or changed
 *   (its tag), in card order (`If-Match`, API-03, S6-20; an ETag `conflict` shows the general text
 *   until S37). What the server took stays saved; a card it refused keeps its input and shows
 *   the errors (a second evidence of a procedure: `candidate.evidence.list.duplicate`, also found
 *   before sending). Success announces `candidate.evidence.saved` (D3 (a)), the focus stays.
 *   Errors follow the S20 pattern of S33: the summary takes the focus and names each card's field
 *   „Prüfnachweis n – …“; the title starts with „Fehler:“.
 * - Only Editor and Owner of a project that is not archived get the form (R-CAN-05, R-PRJ-05);
 *   the Reader view comes with S6-22 (D5 (a)), the publish button with step 6.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2022-267
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2144-3508
 * @spec docs/SPEC_v2.md R-CAN-05; docs/design_handoff_procureaccess_v1.md §5.1
 * @invariants WSP-01, SIG-03, PRV-02, API-02, SEC-10, A11Y-09
 */
@Component({
  selector: 'pa-candidate-evidence',
  imports: [
    ConfirmRequiredNotice,
    ErrorSummary,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatSelectModule,
    NgTemplateOutlet,
    ReactiveFormsModule,
    TextFieldModule,
    TranslatePipe,
  ],
  templateUrl: './candidate-evidence.html',
  styleUrl: './candidate-evidence.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateEvidence {
  private readonly load = inject(CandidateLoad);
  private readonly api = inject(CandidatesApiService);
  private readonly store = inject(ProcureAccessStore);
  private readonly dialog = inject(MatDialog);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);
  private readonly injector = inject(Injector);

  protected readonly statuses = STATUSES;
  protected readonly procedures = TEST_PROCEDURES;
  protected readonly fields = EVIDENCE_FIELDS;
  protected readonly labels = EVIDENCE_LABELS;
  /** `common.date.hint`'s example: today, as the user would type it. */
  protected readonly dateExample = formatGermanDate(todayIsoDate());

  protected readonly status = new FormControl<TestStatus>('unknown', { nonNullable: true });
  protected readonly cards = signal<EvidenceCard[]>([]);
  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);
  /** Whether the shown status is „Ja“; a signal, so the cards follow the radio group. */
  protected readonly tested = signal(false);
  private readonly submitted = signal(false);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');
  /** The element of „Weiteren Prüfnachweis hinzufügen“ (its reference alone would be the MatButton). */
  private readonly addButton = viewChild<string, ElementRef<HTMLElement>>('addButton', { read: ElementRef });

  /** Saved evidences the user removed (with their tags); removed on the server with the next „Speichern“. */
  private removed: TestEvidence[] = [];
  /** The status before the latest change of the radio group, to tell when it leaves „Ja“. */
  private previousStatus: TestStatus = 'unknown';
  /** The candidate the form was filled from; a new candidate in the same tab fills it again. */
  private filledFor: string | null = null;
  private nextKey = 0;
  private destroyed = false;
  private readonly matchers = new WeakMap<EvidenceCard, ErrorStateMatcher>();

  protected readonly candidate = computed<Candidate | null>(() => {
    const view = this.load.view();
    return view.kind === 'candidate' ? view.candidate : null;
  });

  /** The form only for Editor and Owner of a project that is not archived (R-PRJ-05; Readers: S6-22). */
  protected readonly editable = computed(() => {
    const view = this.load.view();
    return (
      view.kind === 'candidate' &&
      view.candidate !== null &&
      view.project.status !== 'archived' &&
      isProjectEditor(this.store.organizations(), view.project)
    );
  });

  constructor() {
    candidateTabTitle(
      'evidence',
      computed(() => this.summary().length > 0),
    );
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
    effect(() => {
      const candidate = this.candidate();
      if (candidate === null || candidate.id === this.filledFor) return;
      untracked(() => this.fill(candidate));
    });
  }

  protected statusId(status: TestStatus): string {
    return `pa-evidence-status-${status}`;
  }

  protected statusKey(status: TestStatus): string {
    return { tested: 'candidate.evidence.tested.yes', notTested: 'candidate.evidence.tested.no', unknown: 'candidate.evidence.tested.unknown' }[status];
  }

  protected fieldId(card: EvidenceCard, field: EvidenceField): string {
    return `pa-evidence-${card.key}-${field}`;
  }

  protected headingId(card: EvidenceCard): string {
    return `pa-evidence-${card.key}-title`;
  }

  /** H3 of a card: „Prüfnachweis n: Art“, or „Prüfnachweis n“ while no kind is chosen (UX copy U179). */
  protected heading(card: EvidenceCard, index: number): TextKey {
    const kind = card.kind.value;
    if (kind === '') return { key: 'candidate.evidence.list.item.untyped', params: { n: index + 1 } };
    const name =
      kind === 'other' && card.otherName.value.trim() !== ''
        ? card.otherName.value.trim()
        : (this.translate.instant(`testType.${kind}`) as string);
    return { key: 'candidate.evidence.list.item', params: { n: index + 1, testType: name } };
  }

  /** The label of a procedure option, also the text of the select's trigger. */
  protected kindKey(kind: KindChoice): string {
    return `testType.${kind}`;
  }

  /** Field errors after a submit and server errors at once, but never on a blank new card (D4 (b)). */
  protected matcher(card: EvidenceCard): ErrorStateMatcher {
    let matcher = this.matchers.get(card);
    if (matcher === undefined) {
      matcher = {
        isErrorState: (control) =>
          !!control?.invalid &&
          (control.hasError('server') || control.hasError('duplicate') || (this.submitted() && !card.isBlank())),
      };
      this.matchers.set(card, matcher);
    }
    return matcher;
  }

  /** The message at a field, also its line of the error summary (E12). */
  protected message(card: EvidenceCard, field: EvidenceField): TextKey | null {
    const control = card.control(field);
    if (control.hasError('server')) return { key: 'errors.field.server', params: { message: control.getError('server') } };
    if (control.hasError('duplicate')) return { key: 'candidate.evidence.list.duplicate' };
    if (control.hasError('required')) {
      return field === 'kind'
        ? { key: 'errors.field.requiredChoice' }
        : { key: 'errors.field.required', params: { field: this.translate.instant(EVIDENCE_LABELS[field]) as string } };
    }
    if (control.hasError('date')) return { key: 'errors.field.date' };
    if (control.hasError('dateFuture')) return { key: 'errors.field.dateFuture' };
    if (control.hasError('url')) return { key: 'errors.field.url' };
    if (control.hasError('maxlength')) {
      const limits: Partial<Record<EvidenceField, number>> = {
        otherName: EVIDENCE_LIMITS.otherTestTypeName,
        testedBy: EVIDENCE_LIMITS.testedBy,
        testedVersion: EVIDENCE_LIMITS.testedVersion,
        evidenceUrl: EVIDENCE_LIMITS.evidenceUrl,
        qualificationNote: EVIDENCE_LIMITS.qualificationNote,
      };
      return { key: 'errors.field.maxLength', params: { max: limits[field], current: control.value.trim().length } };
    }
    return null;
  }

  /** „Andere Prüfung“ makes its name required, another kind drops that rule. */
  protected kindChanged(card: EvidenceCard): void {
    card.otherName.updateValueAndValidity();
  }

  /** The radio group changed: leaving „Ja“ with evidences asks first; choosing „Ja“ shows a card. */
  protected async statusChanged(next: TestStatus): Promise<void> {
    const previous = this.previousStatus;
    if (previous === 'tested' && next !== 'tested') {
      const count = this.cards().filter((card) => !card.isBlank()).length;
      if (count > 0 && !(await this.confirmRemoval(count))) {
        this.status.setValue('tested');
        return;
      }
      this.removed.push(...this.cards().flatMap((card) => (card.saved === null ? [] : [card.saved])));
      this.cards.set([]);
    }
    if (next === 'tested' && this.cards().length === 0) this.cards.set([this.newCard()]);
    this.previousStatus = next;
    this.tested.set(next === 'tested');
  }

  protected add(): void {
    const card = this.newCard();
    this.cards.update((cards) => [...cards, card]);
    afterNextRender(() => this.document.getElementById(this.fieldId(card, 'kind'))?.focus(), { injector: this.injector });
  }

  protected remove(index: number): void {
    const card = this.cards()[index];
    if (card.saved !== null) this.removed.push(card.saved);
    this.cards.update((cards) => cards.filter((other) => other !== card));
    void this.announcer.announce('candidate.evidence.list.removed');
    const previous = this.cards()[index - 1];
    afterNextRender(
      () => (previous ? this.document.getElementById(this.headingId(previous)) : this.addButton()?.nativeElement)?.focus(),
      { injector: this.injector },
    );
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    const candidate = this.candidate();
    if (this.busy() || candidate === null) return;
    this.submitted.set(true);
    this.confirmationRequired.set(false);
    this.markDuplicates();
    const invalid = this.fieldLines();
    if (invalid.length > 0) {
      this.showErrors(invalid);
      return;
    }

    this.busy.set(true);
    const refused: ErrorSummaryItem[] = [];
    try {
      // The status first, with the candidate's tag as it was read: adding or removing an evidence
      // changes that tag (S6-20). Leaving „Ja“ removes the evidences the user removed with it.
      const status = this.status.value;
      if (status !== candidate.testStatus) {
        const removing = status !== 'tested' && this.removed.length > 0;
        if (!(await this.saveStatus(candidate, status, removing))) return;
        if (status !== 'tested') this.removed = [];
      }
      // Then the removals, each with the evidence's own tag, before a card may name its procedure again
      while (this.removed.length > 0) {
        const evidence = this.removed[0];
        try {
          await this.api.removeEvidence(evidence.id, evidence.etag);
        } catch (error: unknown) {
          // Already gone (another session removed it): what the user asked for holds
          if (problemTypeName(error) !== 'not-found') throw error;
        }
        this.removed.shift();
      }
      if (status === 'tested') {
        for (const card of this.cards()) {
          if (!card.needsSaving()) continue;
          try {
            card.saved =
              card.saved === null
                ? await this.api.addEvidence(candidate.id, card.request())
                : await this.api.changeEvidence(card.saved.id, card.request(), card.saved.etag);
          } catch (error: unknown) {
            if (problemTypeName(error) !== 'validation') throw error;
            refused.push(...this.serverErrors(card, error));
          }
        }
      }
    } catch (error: unknown) {
      this.refused(error);
      return;
    } finally {
      this.busy.set(false);
      // Whatever the server took shows on the other tabs too
      await this.refresh(candidate.id);
    }

    this.showErrors(refused);
    if (refused.length === 0) {
      this.submitted.set(false);
      void this.announcer.announce('candidate.evidence.saved');
    }
  }

  /**
   * Sets the test status with the candidate's tag (S6-20). A `test-status-conflict` (evidences on
   * the server that the change would remove) asks with the server's count and repeats the change
   * with `removeEvidences`. False when the user kept them.
   */
  private async saveStatus(candidate: Candidate, status: TestStatus, removeEvidences: boolean): Promise<boolean> {
    try {
      await this.api.changeTestStatus(candidate.id, status, removeEvidences, candidate.etag);
      return true;
    } catch (error: unknown) {
      const count = problemDetails(error)?.evidenceCount ?? 0;
      if (problemTypeName(error) !== 'test-status-conflict' || count === 0) throw error;
      if (!(await this.confirmRemoval(count))) return false;
      await this.api.changeTestStatus(candidate.id, status, true, candidate.etag);
      return true;
    }
  }

  private confirmRemoval(count: number): Promise<boolean> {
    return ConfirmDialog.confirm(this.dialog, {
      titleKey: 'candidate.evidence.list.statusChange.title',
      bodyKey: pluralKey(this.translate, 'candidate.evidence.list.statusChange', count),
      bodyParams: { count: String(count) },
      confirmKey: 'candidate.evidence.list.statusChange.confirm',
    });
  }

  /** Reads the candidate again for S34's other tabs; the cards keep what the user typed. */
  private async refresh(candidateId: string): Promise<void> {
    let candidate: Candidate;
    try {
      candidate = await this.api.get(candidateId);
    } catch {
      return; // the tab keeps its state; the next visit reads the candidate again
    }
    if (this.destroyed) return;
    this.filledFor = candidate.id;
    this.load.view.update((view) => (view.kind === 'candidate' ? { ...view, candidate } : view));
  }

  private fill(candidate: Candidate): void {
    this.filledFor = candidate.id;
    this.removed = [];
    this.submitted.set(false);
    this.summary.set([]);
    this.status.setValue(candidate.testStatus);
    this.previousStatus = candidate.testStatus;
    this.tested.set(candidate.testStatus === 'tested');
    const cards = candidate.testEvidences.map((evidence) => new EvidenceCard(this.key(), evidence));
    this.cards.set(candidate.testStatus === 'tested' && cards.length === 0 ? [this.newCard()] : cards);
  }

  private newCard(): EvidenceCard {
    return new EvidenceCard(this.key());
  }

  private key(): string {
    return String(this.nextKey++);
  }

  /** A catalog procedure a card above already names: the later card shows `candidate.evidence.list.duplicate` (WSP-01). */
  private markDuplicates(): void {
    const seen = new Set<string>();
    // A duplicate found by an earlier „Speichern“ may be gone: check every select afresh
    for (const card of this.cards()) card.kind.updateValueAndValidity();
    for (const card of this.cards()) {
      const kind = card.kind.value;
      if (card.isBlank() || kind === '' || kind === 'other' || kind === 'own') continue;
      if (seen.has(kind)) card.kind.setErrors({ ...card.kind.errors, duplicate: true });
      seen.add(kind);
    }
  }

  /**
   * The field errors of a refused card. A 400 at `testTypeId` for a procedure the select offers
   * can only be a second evidence of it (S6-15): the server's text gives way to the UX copy's.
   */
  private serverErrors(card: EvidenceCard, error: unknown): ErrorSummaryItem[] {
    const index = this.cards().indexOf(card);
    return Object.entries(problemFieldErrors(error)).flatMap(([path, messages]) => {
      const field = EVIDENCE_PATHS[path];
      if (field === undefined) return [{ message: { key: 'errors.field.server', params: { message: messages.join(' ') } } }];
      const control = card.control(field);
      control.setErrors(path === 'testTypeId' ? { duplicate: true } : { server: messages.join(' ') });
      control.markAsTouched();
      return [this.line(card, index, field)];
    });
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.showErrors([]);
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'antiforgery':
        this.showErrors([{ message: { key: 'errors.antiforgery' } }]);
        return;
      case 'validation':
        // Only the test status is sent outside a card; its one field is the radio group
        this.showErrors(
          Object.values(problemFieldErrors(error)).map((messages) => ({
            message: { key: 'errors.field.server', params: { message: messages.join(' ') } },
          })),
        );
        return;
      default:
        // Anything else keeps the system states of the error interceptor (`project-archived` → its note)
        this.showErrors([]);
    }
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
  }

  private line(card: EvidenceCard, index: number, field: EvidenceField): ErrorSummaryItem {
    return {
      message: this.message(card, field) ?? { key: 'errors.field.server', params: { message: '' } },
      field: {
        id: this.fieldId(card, field),
        labelKey: EVIDENCE_LABELS[field],
        group: { key: 'candidate.evidence.list.item.untyped', params: { n: index + 1 } },
      },
    };
  }

  /** The errors of every card that „Speichern“ sends, in card and field order. */
  private fieldLines(): ErrorSummaryItem[] {
    if (!this.tested()) return [];
    return this.cards().flatMap((card, index) =>
      card.isBlank() ? [] : EVIDENCE_FIELDS.filter((field) => this.message(card, field) !== null).map((field) => this.line(card, index, field)),
    );
  }
}
