// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, untracked } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { AssessmentInput } from '@app/shared/components/assessment-input/assessment-input';
import { Notice } from '@app/shared/components/notice/notice';
import { AssessmentValue } from '@app/shared/components/rating-legend/rating-legend';

import { ASSESSMENT_NOTE_LIMIT, AssessmentCriterion, ProcedureAnswer } from '../../../models/assessments';
import { candidatePath } from '../../../models/candidates';
import { CandidatesApiService } from '../../../services/api/candidates-api.service';
import { candidateTabTitle } from '../candidate-detail';
import { CandidateLoad, isProjectEditor } from '../candidate-load';

/**
 * S34b, the tab „Einschätzung nach Kriterien“ at `/projekte/:id/produkte/:pid/einschaetzung`
 * (R-CAN-06, R-CAN-07; Handoff §4.6, §5.1; Figma 360 `2136:3127`, desktop `2022:841`).
 *
 * - The intro `candidate.assessment.intro`, and `candidate.assessment.unionNote` once above the
 *   list when the list's app types are tested by more than one procedure (S6-18 5 (a)).
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
 * - Progress, the filter „Nur noch nicht eingeschätzte“ and the rating legend follow with S6-19b.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2136-3127
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2022-841
 * @spec docs/SPEC_v2.md R-CAN-06, R-CAN-07; docs/design_handoff_procureaccess_v1.md §4.6, §5.1
 * @invariants CAT-15, CAT-17, A11Y-01, A11Y-09
 */
@Component({
  selector: 'pa-candidate-assessment',
  imports: [AssessmentInput, MatButtonModule, Notice, RouterLink, TranslatePipe],
  templateUrl: './candidate-assessment.html',
  styleUrl: './candidate-assessment.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateAssessment {
  private readonly load = inject(CandidateLoad);
  private readonly store = inject(ProcureAccessStore);
  private readonly api = inject(CandidatesApiService);
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
    this.save(criterion, value, criterion.note);
  }

  protected noteChanged(criterion: AssessmentCriterion, note: string): void {
    this.save(criterion, criterion.value, note);
  }

  private save(criterion: AssessmentCriterion, value: AssessmentValue, note: string | null): void {
    const candidateId = this.assessments()?.candidateId;
    if (candidateId == null) return;
    this.changed.add(candidateId);
    this.store.changeAssessment(candidateId, criterion.criterionRevisionId, { value, note });
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
