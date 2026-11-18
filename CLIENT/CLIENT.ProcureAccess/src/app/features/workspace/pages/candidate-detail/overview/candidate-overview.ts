// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { RatingLegend } from '@app/shared/components/rating-legend/rating-legend';
import { FulfillmentBreakdown } from '@app/shared/components/signals/fulfillment-breakdown/fulfillment-breakdown';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import { Candidate, candidatePath, editCandidatePath, TestEvidence, testStatusKey } from '../../../models/candidates';
import { Project } from '../../../models/projects';
import { candidateTabTitle } from '../candidate-detail';
import { CandidateCriteria, CandidateLoad, isProjectEditor } from '../candidate-load';

/** What the overview shows: the loaded project, candidate and copy of the list. */
interface Overview {
  project: Project;
  candidate: Candidate;
  criteria: CandidateCriteria | null;
}

/**
 * S34 Überblick, the default tab of `/projekte/:id/produkte/:pid` (R-CAN-04; Handoff §5.1, Figma 360
 * `2134:3184`, desktop `2097:2627`): facts and the ways to the other tabs, no verdict.
 *
 * Four cards, each a `<section>` without an accessible name under its H2 (owner decision S6-16 D5 (a)):
 * - „Produktangaben“: `<dl>` with the name, vendor, website (a link), summary — an optional fact
 *   that is empty is left out —, the app types of the project's copy („Noch keine
 *   Kriterienliste“ without one) and „Im Produktverzeichnis“ („Nicht verknüpft“ until „Merken“ in
 *   M6); „Bearbeiten“ → S33.
 * - „Stand der Bewertung“: Prüfnachweis (the test status, with evidences „Ja – n Prüfnachweise: …“
 *   as on S32) and „Zum Prüfnachweis“; Einschätzung nach Kriterien with the progress line, K1 in
 *   the workspace scope with seven rows, the rating legend and „Zur Einschätzung“. Without a list
 *   `projects.detail.noCriteria` instead of progress and K1. Two columns from an 840 px container.
 * - „Veröffentlichung“: `candidate.overview.publication.none` and `.explain`; the published states
 *   come in step 6 (`candidate.published.*`).
 * - „Nur für Ihre Organisation“ on surface-container-low: `candidate.form.privateSection.text`, the
 *   contacts and internal notes; „Bearbeiten“ → S33. Never part of a publication (PRV-02).
 *
 * „Bearbeiten“ only for Editor and Owner of a project that is not archived (R-PRJ-05; Readers see
 * every tab read-only with S6-22, R-CAN-10); each is described by its card's heading.
 *
 * Progress and K1 are the server's (S6-18, owner decision S6-16 D3 (b)): requirement criteria
 * only, never „Geprüft nach …“ (CAT-17), seven counts without `notCovered` (SIG-13); the copy is
 * read only for its name, release and app types.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2134-3184
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2097-2627
 * @spec docs/SPEC_v2.md R-CAN-04; docs/design_handoff_procureaccess_v1.md §5.1
 * @invariants SIG-08, SIG-13, PRV-02
 */
@Component({
  selector: 'pa-candidate-overview',
  imports: [FulfillmentBreakdown, MatButtonModule, Notice, RatingLegend, RouterLink, TranslatePipe],
  templateUrl: './candidate-overview.html',
  styleUrl: './candidate-overview.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateOverview {
  private readonly load = inject(CandidateLoad);
  private readonly store = inject(ProcureAccessStore);
  private readonly translate = inject(TranslateService);

  constructor() {
    candidateTabTitle('overview');
  }

  protected readonly overview = computed<Overview | null>(() => {
    const view = this.load.view();
    return view.kind === 'candidate' && view.candidate !== null
      ? { project: view.project, candidate: view.candidate, criteria: view.criteria }
      : null;
  });

  /** „Bearbeiten“: Editor or Owner of the project's organisation, not for an archived project (R-PRJ-05). */
  protected readonly canEdit = computed(() => {
    const overview = this.overview();
    return overview !== null && overview.project.status !== 'archived' && isProjectEditor(this.store.organizations(), overview.project);
  });

  /** Changes when the texts arrive (a direct load may open the page first), so {@link testedStatus} follows. */
  private readonly language = toSignal(this.translate.onLangChange, { initialValue: null });

  /** The `{{status}}` of `candidate.overview.status.tested`: with evidences as S32 names them (U60, U88). */
  protected readonly testedStatus = computed(() => {
    this.language();
    const candidate = this.overview()?.candidate;
    if (candidate === undefined) return '';
    const evidences = candidate.testEvidences;
    if (candidate.testStatus !== 'tested' || evidences.length === 0) {
      return this.translate.instant(testStatusKey(candidate.testStatus)) as string;
    }
    const types = evidences.map((evidence) => this.evidenceTypeName(evidence)).join(', ');
    return this.translate.instant(pluralKey(this.translate, 'projects.detail.row.tested.evidences', evidences.length), {
      count: evidences.length,
      types,
    }) as string;
  });


  protected editPath(overview: Overview): string {
    return editCandidatePath(overview.project.id, overview.candidate.id);
  }

  protected tabPath(overview: Overview, tab: 'evidence' | 'assessment'): string {
    return candidatePath(overview.project.id, overview.candidate.id, tab);
  }

  /** The kind of a test evidence as „Art der Prüfung“ names it (`testType.*`, or the name given to „Andere Prüfung“). */
  private evidenceTypeName(evidence: TestEvidence): string {
    switch (evidence.testKind) {
      case 'other':
        return evidence.otherTestTypeName ?? (this.translate.instant('testType.other') as string);
      case 'own':
        return this.translate.instant('testType.own') as string;
      default:
        return this.translate.instant(`testType.${evidence.testTypeId}`) as string;
    }
  }
}
