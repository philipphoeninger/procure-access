// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  untracked,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaListFilterRow } from '../../components/criteria-list-filter/criteria-list-filter';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { APP_TYPE_STEP_STATE, CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '../../guards/configurator-guards';
import { CriteriaListFilter } from '../../models/criteria-list';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf } from '../../state/criteria-list-rules';

/** Handoff §3.4: counters are announced at most once per second, the newest count wins. */
const RESULTS_THROTTLE_MS = 1000;
const RESULTS_KEY = 'common.results.shown';

/**
 * S11 Kriterienliste at `/kriterien/ergebnis` (R-LST-01 – R-LST-05). S10 opens it after the server
 * accepted the selection (R-CON-10); the guard leads back to S10 when the selection is gone.
 *
 * On every entry it loads the criteria of the draft with the pinned release and shows them as
 * cards in catalog order (`pa-criterion-card`). While the request runs the shell's loading bar
 * shows (`withLoading()`). A 404 opens the legacy filter, like the guard of S10 (O-S4-3); any
 * other failure keeps H1 and „Auswahl ändern“: the error interceptor shows the server page (5xx),
 * the offline notice (no answer) or the generic snackbar (O-S4-34 (a)).
 *
 * Each card's changes — include/exclude, level, weight, reset, note — go into the in-memory draft
 * of `withConfigurator()` (R-LST-03 – R-LST-05); excluded criteria stay in the list, „Geprüft
 * nach …“ cards too (R-LST-07, CAT-16).
 *
 * Above the cards the filter row (`pa-criteria-list-filter`, R-LST-06): every change the user makes
 * announces the number of cards shown once (`common.results.shown`, throttled, the newest count
 * wins); the search counts 300 ms after the last keystroke, when it applies. With no card left the
 * page shows `criteria.list.emptyFilter` and „Filter zurücksetzen“, which clears the filter and puts
 * the focus into the search field.
 *
 * App types without criteria (`appTypesWithoutCriteria[]`, R-LST-08, CAT-20) get the info notice
 * `criteria.list.noCriteriaFor` above the list — a regular state, not an error — with „Auswahl
 * ändern“ to S10 step 2 (owner decision O-S4-47 (a)); with no criterion at all it is the empty state,
 * and the filter row is left out. An empty answer without such app types shows `criteria.list.empty`.
 *
 * Summary, notices, the action bar and „Kriterium vorschlagen“ follow with S4-18. Not
 * `pa-criteria-list`: the legacy list has that selector.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2546 (320 px)
 * @spec docs/SPEC_v2.md R-CON-10, R-LST-01 – R-LST-08 (the rest of the list: R-LST-09 – R-LST-13 with S4-18)
 * @invariants CAT-02, CAT-16, CAT-20
 */
@Component({
  selector: 'pa-criteria-list-page',
  imports: [CriteriaListFilterRow, CriterionCard, MatButtonModule, Notice, RouterLink, TranslatePipe],
  templateUrl: './criteria-list.html',
  styleUrl: './criteria-list.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaListPage {
  protected readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);

  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);
  /** Joins the names of the app types without criteria („A und B“). */
  private readonly appTypeList = new Intl.ListFormat(this.document.documentElement.lang || 'de', {
    type: 'conjunction',
  });
  private readonly filterRow = viewChild(CriteriaListFilterRow);

  protected readonly configuratorPath = CONFIGURATOR_PATH;
  protected readonly appTypeStepState = APP_TYPE_STEP_STATE;
  /** The answer of the current selection, once loaded; null while loading or after a failure. */
  protected readonly answer = computed(() =>
    this.store.configurator.list.status() === 'loaded' ? this.store.configurator.list.answer() : null,
  );
  protected readonly criteria = this.store.visibleCriteria;
  protected readonly names = computed(() => catalogNames(this.store.configurator.options()));
  /** The names of the selected app types without criteria, joined; null when there are none. */
  protected readonly appTypesWithoutCriteria = computed(() => {
    const appTypes = this.answer()?.appTypesWithoutCriteria ?? [];
    return appTypes.length === 0 ? null : this.appTypeList.format(appTypes.map((appType) => appType.name));
  });

  private destroyed = false;
  /** The filter the last announcement counted; undefined until the page has seen one. */
  private announcedFilter: CriteriaListFilter | undefined;

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
    void this.load();

    // A change of the filter while the list is shown is the user's: announce the cards shown. The
    // first filter and one cleared for a new selection (owner decision O-S4-46 (a)) are not announced.
    effect(() => {
      const filter = this.store.configurator.list.filter();
      const shown = this.answer() !== null;
      untracked(() => {
        const changed = this.announcedFilter !== undefined && filter !== this.announcedFilter;
        this.announcedFilter = filter;
        if (changed && shown) this.announceResults();
      });
    });
  }

  /** „Filter zurücksetzen“: the button goes with the empty state, so the focus goes to the search. */
  protected resetFilter(): void {
    this.store.clearCriteriaFilter();
    this.filterRow()?.focusSearch();
  }

  protected decision(key: string) {
    return decisionOf(this.store.configurator.list.decisions(), key);
  }

  private announceResults(): void {
    const count = this.criteria().length;
    void this.announcer.announce(
      pluralKey(this.translate, RESULTS_KEY, count),
      { count },
      { throttleMs: RESULTS_THROTTLE_MS, throttleKey: RESULTS_KEY },
    );
  }

  private async load(): Promise<void> {
    // A user who left S11 while the request ran stays where they went.
    if ((await this.store.loadCriteriaList()) === 'noRelease' && !this.destroyed) {
      await this.router.navigateByUrl(LEGACY_FILTER_PATH);
    }
  }
}
