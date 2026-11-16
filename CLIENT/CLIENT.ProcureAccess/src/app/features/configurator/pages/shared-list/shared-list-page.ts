// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaListSummary } from '../../components/criteria-list-summary/criteria-list-summary';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { CONFIGURATOR_PATH } from '../../guards/configurator-guards';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf, levelCounts } from '../../state/criteria-list-rules';
import { selectionNames } from '../../state/selection-names';
import { SharedListLoad, SharedListView } from './shared-list-load';

/**
 * S16 · Geteilte Kriterienliste at `/geteilt/:token` (R-SAV-05, D7, TEN-09; UX copy §5.6; Handoff
 * §5.3: S11 read only). Readable without an account; the header shows the session as it is (owner
 * decision S5-24 4a). Reached through a link of S15.
 *
 * Shows the list under its own release (CAT-06), in the read-only mode of a saved list (Handoff
 * 1.26; owner decision S5-24 6a): H1 `criteria.shared.title` with the list name, the note
 * `criteria.shared.note` with „Kriterien zusammenstellen“ to S10, the summary, the priority note,
 * the counts and one read-only `pa-criterion-card` per criterion — no checkbox, no K5 menu, no
 * note field, no filter row and no action bar. A link that does not work (malformed, unknown,
 * revoked, expired, its list deleted: one 404, TEN-08) shows „Link nicht mehr gültig“ with
 * `criteria.shared.invalid`; any other failure the S80 server text.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @spec docs/SPEC_v2.md R-SAV-05, §8.5; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants TEN-09, CAT-02, CAT-06, A11Y-09
 */
@Component({
  selector: 'pa-shared-list-page',
  imports: [CriteriaListSummary, CriterionCard, ErrorPage, MatButtonModule, Notice, RouterLink, TranslatePipe, TranslatePluralPipe],
  templateUrl: './shared-list-page.html',
  styleUrl: './shared-list-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SharedListPage {
  private readonly load = inject(SharedListLoad);
  private readonly pageTitle = inject(PageTitleStrategy);

  protected readonly configuratorPath = CONFIGURATOR_PATH;
  protected readonly view = signal<SharedListView>(this.load.view);
  protected readonly shown = computed(() => {
    const view = this.view();
    return view.kind === 'list' ? view : null;
  });
  protected readonly names = computed(() => catalogNames(this.shown()?.options ?? null));
  protected readonly selection = computed(() => {
    const shown = this.shown();
    return shown === null ? null : selectionNames(shown.selection, shown.options);
  });
  protected readonly counts = computed(() => {
    const shown = this.shown();
    return levelCounts(shown?.criteria.criteria ?? [], shown?.decisions ?? {});
  });

  constructor() {
    // Angular keeps the page when only the token changes: take the view the resolver loaded again.
    inject(ActivatedRoute)
      .data.pipe(takeUntilDestroyed())
      .subscribe(() => this.view.set(this.load.view));

    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const name = this.shown()?.list.name;
      if (name !== undefined) {
        this.pageTitle.setPageTitleState({ params: { name } });
      }
    });
  }

  protected decision(key: string) {
    return decisionOf(this.shown()?.decisions ?? {}, key);
  }
}
