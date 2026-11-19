// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { AssessmentInput } from '@app/shared/components/assessment-input/assessment-input';
import { Notice } from '@app/shared/components/notice/notice';
import { ASSESSMENT_VALUES, AssessmentValue, RatingLegend } from '@app/shared/components/rating-legend/rating-legend';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import {
  ASSESSMENT_NOTE_LIMIT,
  AssessmentCriterion,
  ProcedureAnswer,
  SAVED_ANNOUNCEMENT_INTERVAL_MS,
} from '../../../models/assessments';
import { candidatePath } from '../../../models/candidates';
import { CandidatesApiService } from '../../../services/api/candidates-api.service';
import { candidateTabTitle } from '../candidate-detail';
import { CandidateLoad, isProjectEditor } from '../candidate-load';

/** The announcement of the filter's count, as on S11 (`common.results.shown`). */
const RESULTS_KEY = 'common.results.shown';

/** Handoff §3.4: counters are announced at most once per second, the newest count wins (as S11). */
const RESULTS_THROTTLE_MS = 1000;

/**
 * S34b, the tab „Einschätzung nach Kriterien“ at `/projekte/:id/produkte/:pid/einschaetzung`
 * (R-CAN-06, R-CAN-07; Handoff §4.6, §5.1; Figma 360 `2136:3127`, desktop `2022:841`).
 *
 * - The intro `candidate.assessment.intro`, then the progress row (R-CAN-08, S6-19b): „{{done}}
 *   von {{total}} Kriterien eingeschätzt“ — requirement criteria only, counted from the rows as the
 *   server counts (a value other than „Nicht eingeschätzt“), announced when it changes, at most
 *   every 5 s —, the list line `candidate.assessment.list` (S6-19 Q2 (a)) and the filter
 *   „Nur noch nicht eingeschätzte anzeigen“. Ticking it shows the criteria not assessed at that
 *   moment and announces how many (`common.results.shown`); a criterion assessed meanwhile stays
 *   until the filter changes, so the row under the focus never disappears; the „Geprüft nach …“
 *   rows need no assessment and are hidden while it is on; with none left `criteria.list.emptyFilter`.
 * - `pa-rating-legend` with the levels the list allows and „Nicht eingeschätzt“ (Handoff §4.11):
 *   from 600 px expanded unless the user's setting is `collapsed`, below collapsed unless it is
 *   `expanded` (Handoff §2.2). Opening or closing it stores the choice; the user's first assessment
 *   while the setting is `auto` stores `collapsed`, but the legend stays as it is during this visit
 *   (owner decision S6-19b (a): no jump of the list under the focus). The setting lives in
 *   `withSettings()` and on the server (S6-19 Q1 (b)).
 * - `candidate.assessment.unionNote` once directly above the list when the list's app types are
 *   tested by more than one procedure (S6-18 5 (a)).
 * - One K6 per included requirement criterion of the project's list, in list order: its
 *   plain-language title (the title until there is one), only the levels it allows and „Nicht
 *   eingeschätzt“ (CAT-15); radios from 600 px, the compact form below (Figma mapping §2.1).
 * - Each change shows at once and is saved by `withWorkspace()` 800 ms after the row's last change,
 *   announcing `candidate.assessment.saved` at most every 5 s; no spinner (Handoff §6.2). The state
 *   lives in the store, so coming back to the tab shows the last one. Leaving the tab saves what
 *   still waits and then reads the candidate's progress and counts again, so Überblick shows the
 *   server's (C182 (5); never counted on the client).
 * - Then one read-only row per included „Geprüft nach …“ criterion, in list order (= catalog order):
 *   the title in K6's title style, the derived value `assessment.procedure.derived.*` on its own
 *   line, `assessment.procedure.derived.hint` and the link „Prüfnachweis bearbeiten“ to S34a,
 *   described by the title (R-CAN-07, CAT-17: no input, never counted).
 * - Without a list, `projects.detail.noCriteria` (as Überblick). A failed read shows the S80
 *   server text as an error notice in place.
 * - Only Editor and Owner of a project that is not archived get the tab's content (R-PRJ-05); the
 *   Reader view comes with S6-22 (owner decision S6-19 Q3 (a), as S6-17 D5 (a)).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2136-3127
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2022-841
 * @spec docs/SPEC_v2.md R-CAN-06, R-CAN-07, R-CAN-08; docs/design_handoff_procureaccess_v1.md §4.6, §4.11, §5.1
 * @invariants CAT-15, CAT-17, A11Y-01, A11Y-09
 */
@Component({
  selector: 'pa-candidate-assessment',
  imports: [AssessmentInput, MatButtonModule, MatCheckboxModule, Notice, RatingLegend, RouterLink, TranslatePipe],
  templateUrl: './candidate-assessment.html',
  styleUrl: './candidate-assessment.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateAssessment {
  private readonly load = inject(CandidateLoad);
  private readonly store = inject(ProcureAccessStore);
  private readonly api = inject(CandidatesApiService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly wide = injectWideLayout();

  protected readonly noteLimit = ASSESSMENT_NOTE_LIMIT;
  protected readonly layout = computed(() => (this.wide() ? 'radios' : 'compact'));

  /** The project and candidate shown, when the user may assess: Editor or Owner, project not archived. */
  private readonly shown = computed(() => {
    const view = this.load.view();
    if (view.kind !== 'candidate' || view.candidate === null) return null;
    if (view.project.status === 'archived' || !isProjectEditor(this.store.organizations(), view.project)) return null;
    return { project: view.project, candidate: view.candidate };
  });

  protected readonly editable = computed(() => this.shown() !== null);
  protected readonly hasList = computed(() => this.shown()?.project.criteriaList != null);

  /** The assessments of the shown candidate, once read. */
  protected readonly assessments = computed(() => {
    const state = this.store.workspace.assessments();
    const shown = this.shown();
    return shown !== null && state.candidateId === shown.candidate.id ? state : null;
  });

  /** „{{done}} von {{total}}“: the requirement criteria with a value other than „Nicht eingeschätzt“ (R-CAN-08). */
  protected readonly progress = computed(() => {
    const criteria = this.assessments()?.view?.criteria ?? [];
    return { done: criteria.filter((criterion) => criterion.value !== 'unknown').length, total: criteria.length };
  });

  /** The name and release of the project's list for `candidate.assessment.list`. */
  protected readonly listLine = computed(() => {
    const name = this.shown()?.project.criteriaList?.name;
    const release = this.assessments()?.view?.releaseVersion;
    return name != null && release != null ? { list: name, release } : null;
  });

  /** The criteria the filter shows: those not assessed when it was ticked; null while it is off. */
  private readonly filtered = signal<ReadonlySet<number> | null>(null);
  protected readonly filterOn = computed(() => this.filtered() !== null);

  protected readonly shownCriteria = computed(() => {
    const criteria = this.assessments()?.view?.criteria ?? [];
    const filtered = this.filtered();
    return filtered === null ? criteria : criteria.filter((criterion) => filtered.has(criterion.criterionRevisionId));
  });

  /** The levels the list allows, and „Nicht eingeschätzt“, which K6 offers too (Handoff §4.11). */
  protected readonly legendValues = computed<AssessmentValue[]>(() => {
    const allowed = new Set<AssessmentValue>(['unknown']);
    for (const criterion of this.assessments()?.view?.criteria ?? []) {
      for (const value of criterion.allowedRatings) allowed.add(value);
    }
    return ASSESSMENT_VALUES.filter((value) => allowed.has(value));
  });

  /** The user's choice in this visit, or the state fixed at the first assessment; null until either. */
  private readonly legendChoice = signal<boolean | null>(null);
  protected readonly legendExpanded = computed(() => this.legendChoice() ?? this.legendFromSetting());

  protected readonly evidencePath = computed(() => {
    const shown = this.shown();
    return shown === null ? '' : candidatePath(shown.project.id, shown.candidate.id, 'evidence');
  });

  /** The candidates whose rows the user changed in this visit of the tab. */
  private readonly changed = new Set<string>();

  constructor() {
    candidateTabTitle('assessment');

    const candidateId = computed(() => {
      const shown = this.shown();
      return shown !== null && shown.project.criteriaList !== null ? shown.candidate.id : null;
    });
    effect(() => {
      const id = candidateId();
      if (id !== null) untracked(() => void this.store.loadAssessments(id));
    });

    inject(DestroyRef).onDestroy(() => void this.leave());
  }

  protected title(criterion: AssessmentCriterion): string {
    return criterion.plainLanguageTitle ?? criterion.title;
  }

  protected answerKey(procedure: ProcedureAnswer): string {
    return `assessment.procedure.derived.${procedure.answer}`;
  }

  protected procedureTitleId(procedure: ProcedureAnswer): string {
    return `pa-assessment-procedure-${procedure.criterionRevisionId}`;
  }

  protected valueChanged(criterion: AssessmentCriterion, value: AssessmentValue): void {
    if (value !== 'unknown' && this.legendSetting() === 'auto') {
      // The first assessment: from the next visit on collapsed; in this one the legend stays (S6-19b (a))
      this.legendChoice.set(this.legendExpanded());
      this.store.rememberAssessmentLegend('collapsed');
    }
    this.save(criterion, value, criterion.note);
  }

  protected legendToggled(expanded: boolean): void {
    this.legendChoice.set(expanded);
    this.store.rememberAssessmentLegend(expanded ? 'expanded' : 'collapsed');
  }

  /** Shows the criteria not assessed now, or all again; announces how many are shown. */
  protected filterChanged(on: boolean): void {
    const criteria = this.assessments()?.view?.criteria ?? [];
    this.filtered.set(
      on ? new Set(criteria.filter((criterion) => criterion.value === 'unknown').map((criterion) => criterion.criterionRevisionId)) : null,
    );
    const count = this.shownCriteria().length;
    void this.announcer.announce(pluralKey(this.translate, RESULTS_KEY, count), { count }, { throttleMs: RESULTS_THROTTLE_MS, throttleKey: RESULTS_KEY });
  }

  private legendSetting() {
    return this.store.preferences()?.assessmentLegend ?? 'auto';
  }

  /** From 600 px expanded unless collapsed was chosen; below 600 px collapsed unless expanded was (Handoff §2.2). */
  private legendFromSetting(): boolean {
    const setting = this.legendSetting();
    return this.wide() ? setting !== 'collapsed' : setting === 'expanded';
  }

  protected noteChanged(criterion: AssessmentCriterion, note: string): void {
    this.save(criterion, criterion.value, note);
  }

  private save(criterion: AssessmentCriterion, value: AssessmentValue, note: string | null): void {
    const candidateId = this.assessments()?.candidateId;
    if (candidateId == null) return;
    this.changed.add(candidateId);
    const before = this.progress().done;
    this.store.changeAssessment(candidateId, criterion.criterionRevisionId, { value, note });
    const { done, total } = this.progress();
    if (done !== before) {
      void this.announcer.announce(
        'candidate.assessment.progress',
        { done, total },
        { throttleMs: SAVED_ANNOUNCEMENT_INTERVAL_MS },
      );
    }
  }

  /**
   * Saves what still waits, then reads progress and counts of each changed candidate again for
   * Überblick. Only these two: the candidate's tag and fields stay as the other tabs read them
   * (an assessment never changes them, SPEC v2.59-3). A failed read keeps the older counts.
   */
  private async leave(): Promise<void> {
    await this.store.flushAssessments();
    for (const id of this.changed) {
      try {
        const { progress, fulfillment } = await this.api.get(id);
        this.load.view.update((view) =>
          view.kind === 'candidate' && view.candidate?.id === id ? { ...view, candidate: { ...view.candidate, progress, fulfillment } } : view,
        );
      } catch {
        // Überblick keeps the counts it has
      }
    }
  }
}
