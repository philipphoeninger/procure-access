// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, effect, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterOutlet } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';

import { CandidateTabs } from '../../components/candidate-tabs/candidate-tabs';
import { CandidateTab } from '../../models/candidates';
import { CandidateLoad } from './candidate-load';

/**
 * The page title of a tab of S34, „{{tab}} – {{product}} – {{project}}“ (`pageTitle.candidate`,
 * R-CAN-03): the full name of the tab also below 600 px, where the tab shows its short label (U31).
 * Called by each tab, since every tab change is a page change that clears the title's parameters.
 */
export function candidateTabTitle(tab: CandidateTab): void {
  const load = inject(CandidateLoad);
  const pageTitle = inject(PageTitleStrategy);
  // A stream, not `instant`: on a direct load the texts may arrive after the page opened
  const tabName = toSignal(inject(TranslateService).stream(`candidate.tabs.${tab}`) as Observable<string>, { initialValue: '' });
  effect(() => {
    const view = load.view();
    if (view.kind !== 'candidate' || view.candidate === null) return;
    pageTitle.setPageTitleState({ params: { tab: tabName(), product: view.candidate.name, project: view.project.name } });
  });
}

/**
 * S34 „Produkt im Projekt“ at `/projekte/:id/produkte/:pid` (R-CAN-03; Handoff §5.1, Figma 360
 * `2134:3184`, desktop `2097:2627`): the frame around the three tabs.
 *
 * - Title row: the product name as H1. The button „Eintrag veröffentlichen“ with its hint joins
 *   with S35 in step 6 (plan of S6-16).
 * - The vendor line `candidate.vendorLine` („Anbieter: …“, owner decision S6-16 D6 (a)), left out
 *   without a vendor; `projects.archived.note` for an archived project (R-PRJ-05).
 * - `pa-candidate-tabs` with the tabs as routes: Überblick (no suffix, default), Prüfnachweis
 *   (`pruefnachweis`), Einschätzung nach Kriterien (`einschaetzung`). Each tab is a page with its
 *   own title ({@link candidateTabTitle}); the route change moves the focus to the H1.
 * - A project or candidate the user cannot see shows „Seite nicht gefunden“ in place, a failed read
 *   the S80 server text. The breadcrumb of the frame comes with S6-14 (D6 (a)).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2134-3184
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2097-2627
 * @spec docs/SPEC_v2.md R-CAN-03, R-CAN-04; docs/design_handoff_procureaccess_v1.md §5.1
 * @invariants TEN-01, A11Y-09
 */
@Component({
  selector: 'pa-candidate-detail',
  imports: [CandidateTabs, ErrorPage, Notice, RouterOutlet, TranslatePipe],
  templateUrl: './candidate-detail.html',
  styleUrl: './candidate-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CandidateDetailPage {
  private readonly load = inject(CandidateLoad);

  protected readonly view = this.load.view;

  /** The project and candidate shown; null for the S80 page in place. */
  protected readonly shown = computed(() => {
    const view = this.view();
    return view.kind === 'candidate' && view.candidate !== null ? { project: view.project, candidate: view.candidate } : null;
  });

  protected readonly errorKind = computed(() => {
    const view = this.view();
    return view.kind === 'server' ? 'server' : 'notFound';
  });
}
