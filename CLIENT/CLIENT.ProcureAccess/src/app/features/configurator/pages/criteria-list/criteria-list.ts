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
  ElementRef,
  inject,
  untracked,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { injectSuggestionLink } from '@app/core/contact/criteria-suggestion';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { pluralKey, TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaListFilterRow } from '../../components/criteria-list-filter/criteria-list-filter';
import { CriteriaListSummary } from '../../components/criteria-list-summary/criteria-list-summary';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { ExportDialog } from '../../dialogs/export-dialog/export-dialog';
import { APP_TYPE_STEP_STATE, CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '../../guards/configurator-guards';
import { CriteriaListFilter } from '../../models/criteria-list';
import { CriteriaResponse } from '../../models/criteria';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf, includedCount, levelCounts } from '../../state/criteria-list-rules';
import { selectionNames } from '../../state/selection-names';

/** Handoff §3.4: counters are announced at most once per second, the newest count wins. */
const RESULTS_THROTTLE_MS = 1000;
const RESULTS_KEY = 'common.results.shown';
const STATUS_KEY = 'criteria.list.status.count';
/** Handoff §2.1: the action bar is sticky from 600 px width AND 600 px height (same query as the SCSS). */
export const STICKY_ACTION_BAR_QUERY = '(min-width: 37.5em) and (min-height: 37.5em)';

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
 * Above the filter row (S4-18a, Figma order): the summary of the selection
 * (`pa-criteria-list-summary`), the priority note always and the draft note while nobody is signed
 * in (R-LST-09; both at the top, owner decision O-S4-53 (a)), and the counts by level that applies
 * now (`criteria.list.counts`, U106, owner decisions O-S4-50, O-S4-51 (a)). After the list the link
 * „Kriterium vorschlagen“ to the configured channel (R-LST-13). Last the action bar (R-LST-10): the
 * live counter `criteria.list.status.count` — announced, throttled, only when the user includes or
 * excludes a criterion — and „Liste exportieren“, which opens the export dialog S13; „Liste
 * speichern“ and `criteria.list.unsaved` wait for step 3 (owner decision O-S4-49 (a)). The bar is in
 * the flow below 600 px width or height and sticky from there; while it is sticky its height is the
 * page's `scroll-padding-bottom`, so it never covers the focused element (WCAG 2.4.11). Not
 * `pa-criteria-list`: the legacy list has that selector.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2546 (320 px)
 * @spec docs/SPEC_v2.md R-CON-10, R-LST-01 – R-LST-11, R-LST-13
 * @invariants CAT-02, CAT-16, CAT-20
 */
@Component({
  selector: 'pa-criteria-list-page',
  imports: [
    CriteriaListFilterRow,
    CriteriaListSummary,
    CriterionCard,
    MatButtonModule,
    Notice,
    RouterLink,
    TranslatePipe,
    TranslatePluralPipe,
  ],
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
  private readonly dialog = inject(MatDialog);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly filterRow = viewChild(CriteriaListFilterRow);
  private readonly actionBar = viewChild<ElementRef<HTMLElement>>('actionBar');

  protected readonly configuratorPath = CONFIGURATOR_PATH;
  protected readonly appTypeStepState = APP_TYPE_STEP_STATE;
  /** The answer of the current selection, once loaded; null while loading or after a failure. */
  protected readonly answer = computed(() =>
    this.store.configurator.list.status() === 'loaded' ? this.store.configurator.list.answer() : null,
  );
  protected readonly criteria = this.store.visibleCriteria;
  protected readonly names = computed(() => catalogNames(this.store.configurator.options()));
  /** The selection of the shown list by name, for the summary. */
  protected readonly selection = computed(() => {
    const selection = this.store.configurator.list.selection();
    return selection === null ? null : selectionNames(selection, this.store.configurator.options());
  });
  /** Requirement criteria by the level that applies now (owner decision O-S4-51 (a)). */
  protected readonly counts = computed(() =>
    levelCounts(this.answer()?.criteria ?? [], this.store.configurator.list.decisions()),
  );
  /** Cards in the user's list, „Geprüft nach …“ included (`criteria.list.status.count`). */
  protected readonly included = computed(() =>
    includedCount(this.answer()?.criteria ?? [], this.store.configurator.list.decisions()),
  );
  /** From the session (S5-08): the draft note follows every sign-in and sign-out. */
  protected readonly signedIn = this.store.signedIn;
  protected readonly suggest = injectSuggestionLink();
  /** The names of the selected app types without criteria, joined; null when there are none. */
  protected readonly appTypesWithoutCriteria = computed(() => {
    const appTypes = this.answer()?.appTypesWithoutCriteria ?? [];
    return appTypes.length === 0 ? null : this.appTypeList.format(appTypes.map((appType) => appType.name));
  });

  private destroyed = false;
  /** The filter the last announcement counted; undefined until the page has seen one. */
  private announcedFilter: CriteriaListFilter | undefined;
  /** The answer and the count the live counter last saw; a new answer is not announced. */
  private countedAnswer: CriteriaResponse | null = null;
  private countedIncluded = 0;

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

    // The live counter of the action bar (Handoff §5.1: polite, at most once per second): only an
    // include or exclude by the user is announced, never the count of a newly loaded list.
    effect(() => {
      const answer = this.answer();
      const included = this.included();
      untracked(() => {
        const changed = answer !== null && answer === this.countedAnswer && included !== this.countedIncluded;
        this.countedAnswer = answer;
        this.countedIncluded = included;
        if (changed && answer !== null) {
          void this.announcer.announce(
            STATUS_KEY,
            { included, total: answer.criteria.length },
            { throttleMs: RESULTS_THROTTLE_MS },
          );
        }
      });
    });

    this.keepFocusAboveActionBar();
  }

  /** „Liste exportieren“: the export dialog S13 (R-LST-11). */
  protected openExport(): void {
    ExportDialog.open(this.dialog);
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

  /**
   * While the action bar is sticky, its height is the page's `scroll-padding-bottom` (Handoff §2.1,
   * WCAG 2.4.11), so the browser scrolls a focused element above it. The page scrolls, not the
   * sidenav content, so the padding goes on `<html>`; the height follows the bar (it wraps when
   * the window narrows) and the query (it is in the flow below 600 px width or height).
   *
   * Firefox ignores the padding when focus moves: it scrolls only an element outside the viewport,
   * so a control just above the bottom edge would stay behind the bar. When the focus lands on an
   * element the bar overlaps, the page scrolls it above the bar, as far as its top stays in view.
   */
  private keepFocusAboveActionBar(): void {
    const window = this.document.defaultView;
    const root = this.document.documentElement;
    const host = this.host.nativeElement;
    if (!window?.ResizeObserver || !window.matchMedia) return;
    const sticky = window.matchMedia(STICKY_ACTION_BAR_QUERY);
    effect((onCleanup) => {
      const bar = this.actionBar()?.nativeElement;
      if (!bar) return;
      const update = () => {
        root.style.scrollPaddingBottom = sticky.matches ? `${bar.getBoundingClientRect().height}px` : '';
      };
      const reveal = (event: FocusEvent) => {
        const focused = event.target;
        if (!sticky.matches || !(focused instanceof HTMLElement) || bar.contains(focused)) return;
        const box = focused.getBoundingClientRect();
        // An element taller than the space above the bar keeps its top in view.
        const distance = Math.min(box.bottom - bar.getBoundingClientRect().top, box.top);
        if (distance > 0) window.scrollBy({ top: distance });
      };
      const observer = new window.ResizeObserver(update);
      observer.observe(bar);
      sticky.addEventListener('change', update);
      host.addEventListener('focusin', reveal);
      onCleanup(() => {
        observer.disconnect();
        sticky.removeEventListener('change', update);
        host.removeEventListener('focusin', reveal);
        root.style.scrollPaddingBottom = '';
      });
    });
  }

  private async load(): Promise<void> {
    // A user who left S11 while the request ran stays where they went.
    if ((await this.store.loadCriteriaList()) === 'noRelease' && !this.destroyed) {
      await this.router.navigateByUrl(LEGACY_FILTER_PATH);
    }
  }
}
