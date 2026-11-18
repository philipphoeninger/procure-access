// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { skip } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { statusKey } from '../../models/projects';
import { ProjectLoad, ProjectView } from './project-detail-load';

/**
 * S32 „Projektübersicht“ at `/projekte/:id` — the placeholder of S6-13, which S31 opens after
 * creating a project and S30 links by name. It shows only what the existing texts allow: the
 * project name as H1, the line `projects.detail.meta` (status, number of products, visible only
 * to the organisation) and, for an archived project, `projects.archived.note` (R-PRJ-05). The
 * criteria card, the product list, the add-product menu, renaming and the status change come with
 * S6-14 (R-PRJ-03, Figma 360 `2148:4697`, desktop `2148:3877`). A project the user cannot see
 * shows „Seite nicht gefunden“ in place, a failed read the S80 server text.
 *
 * @spec docs/SPEC_v2.md R-PRJ-03 (placeholder), R-PRJ-05
 * @invariants TEN-01
 */
@Component({
  selector: 'pa-project-detail',
  imports: [ErrorPage, Notice, TranslatePipe, TranslatePluralPipe],
  templateUrl: './project-detail.html',
  styleUrl: './project-detail.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProjectDetailPage {
  private readonly load = inject(ProjectLoad);
  private readonly pageTitle = inject(PageTitleStrategy);

  protected readonly view = signal<ProjectView>(this.load.view);

  constructor() {
    // Angular keeps the page when only the id changes: take the view the resolver loaded again.
    inject(ActivatedRoute)
      .paramMap.pipe(skip(1), takeUntilDestroyed())
      .subscribe(() => this.view.set(this.load.view));

    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const view = this.view();
      if (view.kind === 'project') this.pageTitle.setPageTitleState({ params: { project: view.project.name } });
    });
  }

  protected statusKey = statusKey;
}
