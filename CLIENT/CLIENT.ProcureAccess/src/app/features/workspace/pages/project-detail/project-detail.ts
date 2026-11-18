// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { skip } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { pluralKey, TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { projectCriteriaListPath, statusKey } from '../../models/projects';
import { ProjectLoad, ProjectView } from './project-detail-load';

/**
 * S32 „Projektübersicht“ at `/projekte/:id` — the placeholder of S6-13, which S31 opens after
 * creating a project and S30 links by name. It shows only what the existing texts allow: the
 * project name as H1, the line `projects.detail.meta` (status, number of products, visible only
 * to the organisation) and, for an archived project, `projects.archived.note` (R-PRJ-05).
 *
 * Since S6-12 a short section „Kriterienliste“ leads to S11 of the project's own copy (R-PRJ-06;
 * owner decision S6-12 D8 (a)): the list name as a link and its release, and „Kriterienliste
 * anpassen“ for Editor and Owner of an open or finished project; without a list
 * `projects.detail.noCriteria` (its button „Kriterienliste zuordnen“ comes with S6-14). After S11 of
 * the copy took criteria into the list, `projects.detail.criteriaAdded` shows there and is
 * announced once (WSP-03). S6-14 replaces the section by the criteria card of the Handoff.
 *
 * The full criteria card, the product list, the add-product menu, renaming and the status change come with
 * S6-14 (R-PRJ-03, Figma 360 `2148:4697`, desktop `2148:3877`). A project the user cannot see
 * shows „Seite nicht gefunden“ in place, a failed read the S80 server text.
 *
 * @spec docs/SPEC_v2.md R-PRJ-03 (placeholder), R-PRJ-05, R-PRJ-06
 * @invariants TEN-01, WSP-03
 */
@Component({
  selector: 'pa-project-detail',
  imports: [ErrorPage, MatButtonModule, Notice, RouterLink, TranslatePipe, TranslatePluralPipe],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailPage {
  private readonly load = inject(ProjectLoad);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);

  protected readonly view = signal<ProjectView>(this.load.view);
  /** The criteria S11 of the copy took into the list on the way here; 0 shows nothing. */
  protected readonly criteriaAdded = signal(0);

  private readonly project = computed(() => {
    const view = this.view();
    return view.kind === 'project' ? view.project : null;
  });
  /** „Kriterienliste anpassen“: Editor or Owner of the project's organisation, not for an archived project (R-PRJ-05). */
  protected readonly canAdjust = computed(() => {
    const project = this.project();
    if (project === null || project.status === 'archived') return false;
    const role = this.store.organizations().find((organization) => organization.id === project.organizationId)?.role;
    return role === 'owner' || role === 'editor';
  });

  constructor() {
    this.showAddedCriteria();
    // Angular keeps the page when only the id changes: take the view the resolver loaded again.
    inject(ActivatedRoute)
      .paramMap.pipe(skip(1), takeUntilDestroyed())
      .subscribe(() => {
        this.view.set(this.load.view);
        this.showAddedCriteria();
      });

    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const view = this.view();
      if (view.kind === 'project') this.pageTitle.setPageTitleState({ params: { project: view.project.name } });
    });
  }

  protected statusKey = statusKey;
  protected criteriaListPath = projectCriteriaListPath;

  /** `projects.detail.criteriaAdded` once, for the project S11 of its copy reported (R-PRJ-06). */
  private showAddedCriteria(): void {
    const project = this.project();
    const count = project === null ? 0 : this.store.takeCriteriaAdded(project.id);
    this.criteriaAdded.set(count);
    if (count > 0) void this.announcer.announce(pluralKey(this.translate, 'projects.detail.criteriaAdded', count), { count });
  }
}
