// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  Injector,
  untracked,
  viewChild,
  viewChildren,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { injectSuggestionLink } from '@app/core/contact/criteria-suggestion';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@app/features/identity/services/auth.service';
import { ORGANIZATION_PATH } from '@app/features/organization/models/organization';
import { ConfirmDialog } from '@app/shared/components/confirm-dialog/confirm-dialog';
import { Notice } from '@app/shared/components/notice/notice';
import { RatingLegend } from '@app/shared/components/rating-legend/rating-legend';
import { pluralKey, TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaActionBar } from '../../components/criteria-action-bar/criteria-action-bar';
import { CriteriaListFilterRow } from '../../components/criteria-list-filter/criteria-list-filter';
import { CriteriaListSummary } from '../../components/criteria-list-summary/criteria-list-summary';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { ExportDialog } from '../../dialogs/export-dialog/export-dialog';
import { SaveListDialog } from '../../dialogs/save-list-dialog/save-list-dialog';
import { APP_TYPE_STEP_STATE, CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '../../guards/configurator-guards';
import { CriteriaListFilter } from '../../models/criteria-list';
import { CriteriaResponse } from '../../models/criteria';
import { catalogNames } from '../../state/catalog-names';
import { decisionOf, includedCount, levelCounts } from '../../state/criteria-list-rules';
import { ratingsOfList } from '../../state/criterion-detail-rules';
import { selectionNames } from '../../state/selection-names';

/** Handoff §3.4: counters are announced at most once per second, the newest count wins. */
const RESULTS_THROTTLE_MS = 1000;
const RESULTS_KEY = 'common.results.shown';
const STATUS_KEY = 'criteria.list.status.count';

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
 * Between the filter row and the cards the rating legend `pa-rating-legend` (S5-31, Figma `2099:3165`),
 * collapsed, once per page (owner decision #279 (b)): it explains the levels the criteria of the
 * whole list allow, whatever the filter shows, and is left out when they allow none (O-S5-31-1).
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
 * excludes a criterion — „Liste speichern“ (S5-20b) and „Liste exportieren“, which opens the export
 * dialog S13; under the counter `criteria.list.unsaved`, because the draft of S11 is never saved
 * itself — saving makes a list of the organisation (S5-22, owner decision S5-22 4). „Zur Notiz“ of
 * S13 (TD-40) opens the note of its criterion, after clearing a filter that hides its card;
 * „Kriterien neu laden“ of S13 after `release-mismatch` loads the list for the active release, the
 * selection kept and the per-criterion changes dropped, and moves the focus to the H1 (TD-39). The
 * bar (`pa-criteria-action-bar`, shared with a saved list since S5-21b) is in the flow below 600 px
 * width or height and sticky from there, and never covers the focused element (WCAG 2.4.11). Not
 * `pa-criteria-list`: the legacy list has that selector.
 *
 * „Liste speichern“ (R-SAV-01, S5-20b): signed out it opens S01 with `auth.login.reasonSave`; after
 * the sign-in S11 shows `auth.login.draftKept` until the list is saved or S11 is left (owner
 * decision S5-20b 1a). Without a membership it asks for an organisation (`criteria.save.needOrg`,
 * whose action opens S20; owner decision S5-20b 3a); a Reader of the organisation the header shows
 * does not see it; an Editor or Owner gets S12.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2546 (320 px)
 * @spec docs/SPEC_v2.md R-CON-10, R-LST-01 – R-LST-11, R-LST-13, R-SAV-01
 * @invariants CAT-02, CAT-16, CAT-20
 */
@Component({
  selector: 'pa-criteria-list-page',
  imports: [
    CriteriaActionBar,
    CriteriaListFilterRow,
    CriteriaListSummary,
    CriterionCard,
    MatButtonModule,
    Notice,
    RatingLegend,
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
  private readonly auth = inject(AuthService);
  private readonly filterRow = viewChild(CriteriaListFilterRow);
  private readonly cards = viewChildren(CriterionCard);
  private readonly injector = inject(Injector);
  private readonly pageTitle = inject(PageTitleStrategy);

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
  /** The levels the rating legend above the list explains, from the whole list (O-S5-31-1). */
  protected readonly ratings = computed(() => ratingsOfList(this.answer()?.criteria ?? []));
  /** Cards in the user's list, „Geprüft nach …“ included (`criteria.list.status.count`). */
  protected readonly included = computed(() =>
    includedCount(this.answer()?.criteria ?? [], this.store.configurator.list.decisions()),
  );
  /** From the session (S5-08): the draft note follows every sign-in and sign-out. */
  protected readonly signedIn = this.store.signedIn;
  protected readonly suggest = injectSuggestionLink();
  /**
   * What „Liste speichern“ does (R-SAV-01): sign in first, ask for an organisation, open S12 — or
   * nothing, for a Reader of the organisation the header shows, who does not see the button.
   */
  protected readonly saveAction = computed<'signIn' | 'needOrganization' | 'save' | null>(() => {
    if (!this.signedIn()) return 'signIn';
    const organization = this.store.activeOrganization();
    if (organization === null) return 'needOrganization';
    return organization.role === 'reader' ? null : 'save';
  });
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
    inject(DestroyRef).onDestroy(() => {
      this.destroyed = true;
      // The hint after the sign-in belongs to this visit of S11 (owner decision S5-20b 1a).
      this.store.forgetDraftKept();
    });
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

  }

  /** „Liste exportieren“: the export dialog S13 (R-LST-11). */
  protected openExport(): void {
    ExportDialog.open(this.dialog)
      .afterClosed()
      .subscribe((result) => {
        // „Abbrechen“, „Schließen“ and Esc close without a result object (`mat-dialog-close` gives '').
        if (typeof result !== 'object') return;
        if ('toNote' in result) this.showNote(result.toNote);
        else void this.reloadForActiveRelease();
      });
  }

  /**
   * „Kriterien neu laden“ of S13 after `release-mismatch` (TD-39, #310; owner decision O-S6-5): the
   * list for the active release with the same selection and without the per-criterion changes. The
   * button that opened S13 goes while the list loads, so the focus moves to the H1 afterwards; no
   * release at all opens the legacy filter, as on entry.
   */
  private async reloadForActiveRelease(): Promise<void> {
    const status = await this.store.reloadCriteriaListForActiveRelease();
    if (this.destroyed) return;
    if (status === 'noRelease') {
      await this.router.navigateByUrl(LEGACY_FILTER_PATH);
      return;
    }
    this.pageTitle.focusPageHeading();
  }

  /** „Zur Notiz“ of S13 (TD-40): the card opens its note; a filter that hides the card is cleared first. */
  private showNote(key: string): void {
    if (!this.criteria().some((criterion) => criterion.key === key)) {
      this.store.clearCriteriaFilter();
    }
    afterNextRender(() => this.cards().find((card) => card.criterion().key === key)?.showNote(), {
      injector: this.injector,
    });
  }

  /** „Liste speichern“ (R-SAV-01): see {@link saveAction}. */
  protected async save(): Promise<void> {
    switch (this.saveAction()) {
      case 'signIn':
        await this.auth.showLogin('save');
        return;
      case 'needOrganization':
        if (
          await ConfirmDialog.confirm(this.dialog, {
            titleKey: 'criteria.save.title',
            bodyKey: 'criteria.save.needOrg',
            confirmKey: 'criteria.save.needOrg.cta',
          })
        ) {
          // The draft stays in memory; the way back is „Kriterien“ (owner decision S5-20b 3a).
          await this.router.navigate(['/', ORGANIZATION_PATH]);
        }
        return;
      case 'save':
        SaveListDialog.open(this.dialog);
        return;
      default:
        return;
    }
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
