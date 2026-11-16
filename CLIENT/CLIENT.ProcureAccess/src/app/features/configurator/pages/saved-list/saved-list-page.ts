// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { skip } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorPage } from '@app/pages/error/error-page';
import { Notice } from '@app/shared/components/notice/notice';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaListSummary } from '../../components/criteria-list-summary/criteria-list-summary';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf, levelCounts } from '../../state/criteria-list-rules';
import { selectionNames } from '../../state/selection-names';
import { SavedListLoad, SavedListView } from './saved-list-load';

/**
 * A saved list at `/kriterienlisten/:id` — S11 read only (R-SAV-02: „opening a list shows S11 with
 * its name as H1“; owner decision S5-20 5a: read only until S5-21 makes it editable). Reached from
 * S14 and after saving (S12, owner decision S5-20 4a).
 *
 * As S11, from its own release (CAT-06): H1 the list name, the summary of the selection, the
 * priority note, the counts by the level that applies, and one `pa-criterion-card` per criterion in
 * catalog order with `readonly` — the stored decision without checkbox, menu or note field; an
 * excluded criterion keeps „Nicht in Ihrer Liste“. No filter row, no „Auswahl ändern“, no action bar
 * (as the shared list S16 will be, R-SAV-05). The draft of S10/S11 is not touched.
 *
 * The resolver loads the list, so the title „{{name}} – Gespeicherte Kriterienlisten“ is known at
 * the route change; a list that does not exist for the user shows „Seite nicht gefunden“ in place.
 *
 * @spec docs/SPEC_v2.md R-SAV-02, §8.10; docs/design_handoff_procureaccess_v1.md §5.1 S11, §5.3
 * @invariants CAT-02, CAT-06, TEN-01, A11Y-09
 */
@Component({
  selector: 'pa-saved-list-page',
  imports: [CriteriaListSummary, CriterionCard, ErrorPage, Notice, TranslatePipe, TranslatePluralPipe],
  templateUrl: './saved-list-page.html',
  styleUrl: './saved-list-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedListPage {
  private readonly load = inject(SavedListLoad);
  private readonly pageTitle = inject(PageTitleStrategy);

  protected readonly view = signal<SavedListView>(this.load.view);
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
    // Angular keeps the page when only the id changes (a link from one list to another): take the
    // view the resolver loaded again.
    inject(ActivatedRoute)
      .paramMap.pipe(skip(1), takeUntilDestroyed())
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
