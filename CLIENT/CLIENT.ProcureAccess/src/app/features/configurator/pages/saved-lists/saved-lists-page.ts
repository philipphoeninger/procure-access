// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, DestroyRef, effect, inject, signal, untracked } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ORGANIZATION_PATH } from '@app/features/organization/models/organization';
import { injectWideLayout } from '@app/layout/shell/wide-layout';
import { ErrorPage } from '@app/pages/error/error-page';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import { CONFIGURATOR_PATH } from '../../guards/configurator-guards';
import { savedListPath, SavedListSort, SavedListSummary } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';

/** What S14 shows below its H1. */
type SavedListsStatus = 'loading' | 'loaded' | 'noOrganization' | 'failed';

/** The options of „Sortieren nach“ in their order; the first is preselected (UX copy U141). */
export const SAVED_LIST_SORTS: readonly SavedListSort[] = ['modified', 'name'];

/**
 * The lists in the chosen order: the most recently changed first, or by name (German collation),
 * each with the other as the tie-breaker. Never by anything else (Handoff §5.3 S14: name or date).
 */
export function sortSavedLists(lists: readonly SavedListSummary[], sort: SavedListSort, collator: Intl.Collator): SavedListSummary[] {
  const byModified = (a: SavedListSummary, b: SavedListSummary) => Date.parse(b.modifiedAt) - Date.parse(a.modifiedAt);
  const byName = (a: SavedListSummary, b: SavedListSummary) => collator.compare(a.name, b.name);
  return [...lists].sort(sort === 'name' ? (a, b) => byName(a, b) || byModified(a, b) : (a, b) => byModified(a, b) || byName(a, b));
}

/**
 * S14 „Gespeicherte Kriterienlisten“ at `/kriterienlisten` (R-SAV-02; UX copy §5.6, U140–U142;
 * Handoff §5.3: no Figma frame, list as S30), signed in, reached from „Kriterienlisten“ in the menu
 * „Konto“ (owner decision S5-20b 4a).
 *
 * - The lists of the organisation the header shows (organisation lists only; project copies come
 *   with step 5), loaded again when the header switches the organisation (R-SHL-03, TEN-01). After
 *   every load `common.loaded` announces the number.
 * - „Sortieren nach“ above the list in every width (owner decision S5-20b 2a): „Zuletzt geändert“
 *   (preselected, newest first) or „Name“; a change announces `criteria.saved.sort.updated`.
 * - Below 600 px a list of cards (name as H2 link, then „Zuletzt geändert“ and „Katalogversion“ as a
 *   description list), from 600 px a table with the same three columns, without sortable headers.
 *   The name opens the list (read only until S5-21). No creator (owner decision S5-20 3a).
 * - No list: `criteria.saved.empty` with „Kriterien zusammenstellen“. No membership: the text
 *   `criteria.save.needOrg` with „Organisation anlegen oder beitreten“ to S20. A failed load shows
 *   the S80 server text in place.
 *
 * @spec docs/SPEC_v2.md R-SAV-02, R-SHL-03, §8.10; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants TEN-01, A11Y-09
 */
@Component({
  selector: 'pa-saved-lists-page',
  imports: [ErrorPage, MatButtonModule, MatFormFieldModule, MatSelectModule, MatTableModule, RouterLink, TranslatePipe],
  templateUrl: './saved-lists-page.html',
  styleUrl: './saved-lists-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedListsPage {
  private readonly store = inject(ProcureAccessStore);
  private readonly api = inject(ConfiguratorApiService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly language = inject(DOCUMENT).documentElement.lang || 'de';
  private readonly collator = new Intl.Collator(this.language);
  private readonly dateFormat = new Intl.DateTimeFormat(this.language, { dateStyle: 'medium' });

  protected readonly wide = injectWideLayout();
  protected readonly sorts = SAVED_LIST_SORTS;
  protected readonly columns = ['name', 'modified', 'release'];
  protected readonly configuratorPath = CONFIGURATOR_PATH;
  protected readonly organizationPath = `/${ORGANIZATION_PATH}`;
  protected readonly titleId = 'pa-saved-lists-title';

  protected readonly status = signal<SavedListsStatus>('loading');
  private readonly lists = signal<SavedListSummary[]>([]);
  protected readonly sort = signal<SavedListSort>('modified');
  protected readonly sorted = computed(() => sortSavedLists(this.lists(), this.sort(), this.collator));

  private latestLoad = 0;
  private destroyed = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));

    // The organisation the header shows; a switch loads its lists (R-SHL-03).
    const organizationId = computed(() => this.store.activeOrganization()?.id ?? null);
    effect(() => {
      const id = organizationId();
      untracked(() => void this.load(id));
    });
  }

  protected path(list: SavedListSummary): string {
    return savedListPath(list.id);
  }

  protected date(value: string): string {
    return this.dateFormat.format(new Date(value));
  }

  protected sortKey(sort: SavedListSort): string {
    return `criteria.saved.sort.${sort}`;
  }

  /** „Sortieren nach“: the new order is announced (U141). */
  protected chooseSort(sort: SavedListSort): void {
    this.sort.set(sort);
    void this.announcer.announce('criteria.saved.sort.updated', { sort: this.translate.instant(this.sortKey(sort)) as string });
  }

  private async load(organizationId: string | null): Promise<void> {
    const request = ++this.latestLoad;
    if (organizationId === null) {
      this.lists.set([]);
      this.status.set('noOrganization');
      return;
    }
    this.status.set('loading');
    this.store.incrementLoadingCount();
    try {
      const lists = await this.api.list(organizationId);
      if (this.destroyed || request !== this.latestLoad) return;
      this.lists.set(lists);
      this.status.set('loaded');
      void this.announcer.announce(pluralKey(this.translate, 'common.loaded', lists.length), { count: lists.length });
    } catch {
      if (this.destroyed || request !== this.latestLoad) return;
      this.status.set('failed');
    } finally {
      this.store.decrementLoadingCount();
    }
  }
}
