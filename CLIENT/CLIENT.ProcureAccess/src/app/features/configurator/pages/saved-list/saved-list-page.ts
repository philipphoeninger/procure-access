// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  Injector,
  signal,
  untracked,
  viewChild,
  viewChildren,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { skip } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorPage } from '@app/pages/error/error-page';
import { ConflictDialog } from '@app/shared/components/conflict-dialog/conflict-dialog';
import { ChangedField, ChangedGroup, changesText } from '@app/shared/components/conflict-dialog/edit-conflict';
import { Notice } from '@app/shared/components/notice/notice';
import { RatingLegend } from '@app/shared/components/rating-legend/rating-legend';
import { pluralKey, TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { CriteriaActionBar } from '../../components/criteria-action-bar/criteria-action-bar';
import { CriteriaListFilterRow } from '../../components/criteria-list-filter/criteria-list-filter';
import { CriteriaListSummary } from '../../components/criteria-list-summary/criteria-list-summary';
import { CriterionCard } from '../../components/criterion-card/criterion-card';
import { ExportDialog } from '../../dialogs/export-dialog/export-dialog';
import { ShareDialog } from '../../dialogs/share-dialog/share-dialog';
import { Criterion, PriorityLevel } from '../../models/criteria';
import { CriteriaListFilter } from '../../models/criteria-list';
import { SavedListCriterionName, savedListPath, savedListSelectionPath } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { catalogNames } from '../../state/catalog-names';
import {
  decisionOf,
  effectiveLevel,
  effectiveWeight,
  filterCriteria,
  firstTooLongNote,
  includedCount,
  levelCounts,
} from '../../state/criteria-list-rules';
import { procedureTestType, requirementTitle } from '../../state/criterion-card-rules';
import { ratingsOfList } from '../../state/criterion-detail-rules';
import { selectionNames } from '../../state/selection-names';
import { CRITERIA_SEARCH_DEBOUNCE_MS } from '../../state/with-configurator';
import { SavedListEditor, UnsavedDecision } from './saved-list-editor';
import { SavedListArrival, SavedListLoad, SavedListNavigationState, SavedListView } from './saved-list-load';

/** Handoff §3.4: counters are announced at most once per second, the newest count wins. */
const RESULTS_THROTTLE_MS = 1000;
const RESULTS_KEY = 'common.results.shown';
const STATUS_KEY = 'criteria.list.status.count';

/**
 * A saved list at `/kriterienlisten/:id` — S11 of a list of the organisation (R-SAV-02, R-SAV-03,
 * R-LST-14). Reached from S14, after saving (S12), after „Auswahl ändern“ and after „Neue Liste mit
 * derselben Auswahl“. Shows the list under its own release (CAT-06): H1 the list name, the summary,
 * the priority note, the counts, the filter row (R-LST-06), the rating legend `pa-rating-legend` as on
 * S11 (S5-31; collapsed, the levels of the whole list, left out when they allow none; owner decisions
 * #279 (b), O-S5-31-1, O-S5-31-2) and one `pa-criterion-card` per criterion.
 *
 * **Editor and Owner** of the list's organisation change it here (S5-21b): the cards decide as on
 * S11, and {@link SavedListEditor} saves each change right away (owner decision S5-21b 1a). Below
 * the H1, as on S11 (Figma `2020:70`), „Auswahl ändern“ opens S10 at `/kriterienlisten/:id/auswahl`
 * (owner decision S5-21b 2a), not for a list of an older release (owner decision S5-21 5a); the
 * action bar has the counter and „Liste exportieren“.
 * On a list of an older release the notice `criteria.list.olderRelease` offers „Neue Liste mit
 * derselben Auswahl“, which copies the list to the active release and opens the copy with its
 * report (R-LST-14, CAT-22).
 *
 * „Freigabelink erstellen“ next to it opens S15 with the list's share links (R-SAV-04; owner
 * decision S5-24 2a), also on a list of an older release.
 *
 * **Readers** see the cards read only, the filter row and an action bar with „Liste exportieren“
 * only (owner decision S5-21b 6a), and no older-release notice (owner decision S5-21b 5b).
 *
 * A report handed in the navigation state (`SavedListArrival`) shows once as an info notice and is
 * announced: the copy (`criteria.list.copy.*`) or the decisions a selection change dropped
 * (`criteria.list.selectionChanged.dropped`, owner decision S5-21 4a).
 *
 * The resolver loads the list, so the title „{{name}} – Gespeicherte Kriterienlisten“ is known at
 * the route change; a list that does not exist for the user shows „Seite nicht gefunden“ in place.
 *
 * **Project-copy mode** (R-PRJ-06, WSP-03; S6-12) at `/projekte/:id/kriterienliste`, the project's
 * own copy of a list: the notice `criteria.list.projectCopy` names the project; the four selection
 * layers are fixed, so „Auswahl ändern“, „Freigabelink erstellen“ and the older-release notice with
 * „Neue Liste mit derselben Auswahl“ are not offered (the server answers 409 `project-copy`, D2 (a));
 * including, excluding, priorities and notes are changed as on any list. The copy of an archived
 * project is read-only, with `projects.archived.note` (owner decision S6-12 D3 (a)). The title is
 * „Kriterienliste – {{project}} – Beschaffungsprojekte“ (D6 (a)). Leaving the page reports to the
 * workspace how many criteria were taken into the list, for `projects.detail.criteriaAdded` on the
 * next S32 of the project.
 *
 * **Conflict** (R-CAN-11, API-03; S6-21b, owner decision S6-21 D3 (a)): when someone else changed
 * the list since it was read, the refused save opens S37 (Figma `2056:2519`). The shown decisions
 * stay; nothing more is saved, and every further change opens S37 again. „Meine Änderungen
 * kopieren“ copies each changed criterion under its title with its changed parts (UX copy U189);
 * „Neue Fassung laden“ reads the list again as the route did (`SavedListLoad.reload`), shows it and
 * moves the focus to the H1.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2020-70
 * @spec docs/SPEC_v2.md R-SAV-02, R-SAV-03, R-SAV-04, R-LST-06, R-LST-10, R-LST-14, R-PRJ-06, §8.10; docs/design_handoff_procureaccess_v1.md §5.1 S11, §5.3
 * @invariants CAT-02, CAT-06, CAT-22, TEN-01, TEN-08, WSP-03, A11Y-09
 */
@Component({
  selector: 'pa-saved-list-page',
  imports: [
    CriteriaActionBar,
    CriteriaListFilterRow,
    CriteriaListSummary,
    CriterionCard,
    ErrorPage,
    MatButtonModule,
    Notice,
    RatingLegend,
    RouterLink,
    TranslatePipe,
    TranslatePluralPipe,
  ],
  templateUrl: './saved-list-page.html',
  styleUrl: './saved-list-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SavedListPage {
  private readonly load = inject(SavedListLoad);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly api = inject(ConfiguratorApiService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly filterRow = viewChild(CriteriaListFilterRow);
  private readonly cards = viewChildren(CriterionCard);
  private readonly injector = inject(Injector);
  protected readonly editor = inject(SavedListEditor);

  protected readonly view = signal<SavedListView>({ kind: 'notFound' });
  protected readonly shown = computed(() => {
    const view = this.view();
    return view.kind === 'list' ? view : null;
  });
  protected readonly names = computed(() => catalogNames(this.shown()?.options ?? null));
  protected readonly selection = computed(() => {
    const shown = this.shown();
    return shown === null ? null : selectionNames(shown.selection, shown.options);
  });
  private readonly criteria = computed(() => this.shown()?.criteria.criteria ?? []);
  protected readonly decisions = this.editor.decisions;
  protected readonly counts = computed(() => levelCounts(this.criteria(), this.decisions()));
  protected readonly included = computed(() => includedCount(this.criteria(), this.decisions()));
  /** The levels the rating legend above the list explains, from the whole list (O-S5-31-1). */
  protected readonly ratings = computed(() => ratingsOfList(this.criteria()));

  /** The project of a project's copy (S6-12); null for a list of the organisation. */
  protected readonly project = computed(() => this.shown()?.project ?? null);
  /**
   * Editor or Owner of the list's organisation (the header may show another one); never for the
   * copy of an archived project (owner decision S6-12 D3 (a)).
   */
  protected readonly canEdit = computed(() => {
    const organizationId = this.shown()?.list.organizationId;
    const role = this.store.organizations().find((organization) => organization.id === organizationId)?.role;
    return (role === 'owner' || role === 'editor') && this.project()?.archived !== true;
  });
  /** „Auswahl ändern“, „Freigabelink erstellen“ and a new list of the older release: lists of the organisation only. */
  protected readonly listActions = computed(() => this.canEdit() && this.project() === null);
  /** The list is on an older release than the active one (R-LST-14); only Editor and Owner see it (S5-21b 5b). */
  protected readonly olderRelease = computed(() => {
    const shown = this.shown();
    return shown !== null && shown.activeRelease !== null && shown.activeRelease !== shown.list.releaseVersion;
  });
  protected readonly changeSelectionPath = computed(() => {
    const id = this.shown()?.list.id;
    return id === undefined ? null : savedListSelectionPath(id);
  });

  protected readonly searchInput = signal('');
  private readonly query = signal('');
  protected readonly levels = signal<PriorityLevel[]>([]);
  protected readonly onlyIncluded = signal(false);
  private readonly filter = computed<CriteriaListFilter>(() => ({
    query: this.query(),
    levels: this.levels(),
    onlyIncluded: this.onlyIncluded(),
  }));
  protected readonly visible = computed(() => filterCriteria(this.criteria(), this.decisions(), this.filter()));

  protected readonly arrival = signal<SavedListArrival | null>(null);
  protected readonly copying = signal(false);

  private searchTimer: ReturnType<typeof setTimeout> | undefined;
  /** S37 is open: further conflicts of the editor do not open it a second time. */
  private conflictShown = false;
  private destroyed = false;
  /** The filter the last announcement counted; a change by the user announces the cards shown. */
  private announcedFilter: CriteriaListFilter | undefined;
  private countedView: SavedListView | null = null;
  private countedIncluded = 0;

  constructor() {
    this.show();
    // Angular keeps the page when only the id changes (a link from one list to another, a copy):
    // take the view the resolver loaded again.
    inject(ActivatedRoute)
      .paramMap.pipe(skip(1), takeUntilDestroyed())
      .subscribe(() => this.show());

    this.editor.conflicts.pipe(takeUntilDestroyed()).subscribe(() => void this.showConflict());

    inject(DestroyRef).onDestroy(() => {
      this.destroyed = true;
      clearTimeout(this.searchTimer);
      this.editor.flush();
      this.reportAddedCriteria();
    });

    // The route change clears the title state; the name is known from the resolver.
    effect(() => {
      const shown = this.shown();
      if (shown === null) return;
      this.pageTitle.setPageTitleState({
        params: shown.project === null ? { name: shown.list.name } : { project: shown.project.name },
      });
    });

    effect(() => {
      const filter = this.filter();
      untracked(() => {
        const changed = this.announcedFilter !== undefined && filter !== this.announcedFilter;
        this.announcedFilter = filter;
        if (changed) this.announceResults();
      });
    });

    // The live counter (Handoff §5.1): only an include or exclude by the user is announced.
    effect(() => {
      const view = this.view();
      const included = this.included();
      untracked(() => {
        const changed = view === this.countedView && included !== this.countedIncluded;
        this.countedView = view;
        this.countedIncluded = included;
        if (changed) {
          void this.announcer.announce(
            STATUS_KEY,
            { included, total: this.criteria().length },
            { throttleMs: RESULTS_THROTTLE_MS },
          );
        }
      });
    });
  }

  protected decision(key: string) {
    return decisionOf(this.decisions(), key);
  }

  protected search(text: string): void {
    this.searchInput.set(text);
    clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.query.set(text), CRITERIA_SEARCH_DEBOUNCE_MS);
  }

  /** „Filter zurücksetzen“: the button goes with the empty state, so the focus goes to the search. */
  protected resetFilter(): void {
    this.clearFilter();
    this.filterRow()?.focusSearch();
  }

  private clearFilter(): void {
    clearTimeout(this.searchTimer);
    this.searchInput.set('');
    this.query.set('');
    this.levels.set([]);
    this.onlyIncluded.set(false);
  }

  /** „Liste exportieren“: S13 for the stored list (R-LST-12), once every change is saved. */
  protected async openExport(): Promise<void> {
    await this.editor.settled();
    const shown = this.shown();
    if (shown === null) return;
    ExportDialog.open(this.dialog, {
      listId: shown.list.id,
      release: shown.list.releaseVersion,
      included: this.included(),
      // A too-long note is never stored (S5-21b R4): S13 leads to it instead (TD-40).
      tooLongNote: firstTooLongNote(this.criteria(), this.decisions()),
    })
      .afterClosed()
      .subscribe((result) => {
        // A saved list exports under its own release, so S13 never offers „Kriterien neu laden“ here.
        // „Abbrechen“ and Esc close without a result object (`mat-dialog-close` gives '').
        if (typeof result === 'object' && 'toNote' in result) this.showNote(result.toNote);
      });
  }

  /** „Freigabelink erstellen“: S15 for the stored list (R-SAV-04); MatDialog returns the focus here. */
  protected openShare(): void {
    const shown = this.shown();
    if (shown !== null) ShareDialog.open(this.dialog, { listId: shown.list.id });
  }

  /** „Zur Notiz“ of S13 (TD-40): the card opens its note; a filter that hides the card is cleared first. */
  private showNote(key: string): void {
    if (!this.visible().some((criterion) => criterion.key === key)) {
      this.clearFilter();
    }
    afterNextRender(() => this.cards().find((card) => card.criterion().key === key)?.showNote(), {
      injector: this.injector,
    });
  }

  /**
   * „Neue Liste mit derselben Auswahl“ (R-LST-14, CAT-22): the copy on the active release opens with
   * its report. A failure leaves the page as it is; the error interceptor shows it.
   */
  protected async copyToActiveRelease(): Promise<void> {
    const shown = this.shown();
    if (shown === null || this.copying()) return;
    this.copying.set(true);
    try {
      await this.editor.settled();
      const copy = await this.api.copyToActiveRelease(shown.list.id);
      const state: SavedListNavigationState = {
        savedListArrival: {
          kind: 'copied',
          releaseVersion: copy.releaseVersion,
          droppedValues: copy.droppedValues,
          notCarried: copy.notCarried,
        },
      };
      await this.router.navigateByUrl(savedListPath(copy.id), { state });
    } catch {
      // Shown by the error interceptor.
    } finally {
      this.copying.set(false);
    }
  }

  /** The criteria of a report, joined with commas: number and title, or the title alone. */
  protected criteriaOf(criteria: readonly SavedListCriterionName[]): string {
    return criteria.map((criterion) => (criterion.number ? `${criterion.number} ${criterion.title}` : criterion.title)).join(', ');
  }

  /**
   * The criteria of a project's copy that were left out when the page showed it and are in the
   * list now (R-PRJ-06, WSP-03): reported to the workspace for the next S32 of the project.
   */
  private reportAddedCriteria(): void {
    const shown = this.shown();
    if (shown === null || shown.project === null || !this.canEdit()) return;
    const now = this.decisions();
    const added = shown.criteria.criteria.filter(
      (criterion) => !decisionOf(shown.decisions, criterion.key).included && decisionOf(now, criterion.key).included,
    ).length;
    this.store.noteCriteriaAdded(shown.project.id, added);
  }

  /** S37 with the unsaved decisions; „Neue Fassung laden“ reads the list again (S6-21b). */
  private async showConflict(): Promise<void> {
    if (this.conflictShown || this.destroyed) return;
    this.conflictShown = true;
    let reload: boolean;
    try {
      reload = await ConflictDialog.show(this.dialog, {
        editorDisplayName: this.editor.conflictEditor,
        changes: changesText(this.editor.unsaved().map((unsaved) => this.changedGroup(unsaved))),
      });
    } finally {
      this.conflictShown = false;
    }
    if (reload && !this.destroyed) await this.reload();
  }

  /**
   * One criterion of the copied text (UX copy U189): its title as the card shows it, then what
   * changed — included, the level that applies, the weight, the note — with the card's labels.
   */
  private changedGroup({ criterion, saved, shown }: UnsavedDecision): ChangedGroup {
    const text = (key: string, params?: Record<string, unknown>) => this.translate.instant(key, params) as string;
    const fields: ChangedField[] = [];
    if (shown.included !== saved.included) {
      fields.push({ label: text('criteria.item.include'), value: text(shown.included ? 'common.yes' : 'common.no') });
    }
    const level = effectiveLevel(criterion, shown);
    if (level !== effectiveLevel(criterion, saved)) {
      fields.push({ label: text('criteria.priority.changeLabel'), value: text(`criteria.priority.${level}`) });
    }
    const weight = effectiveWeight(criterion, shown);
    if (weight !== null && weight !== effectiveWeight(criterion, saved)) {
      fields.push({ label: text('criteria.priority.weightLabel'), value: String(weight) });
    }
    if (shown.note !== saved.note) fields.push({ label: text('criteria.item.note'), value: shown.note });
    return { heading: this.cardTitle(criterion), fields };
  }

  /** The H2 of a card: the plain-language title, or „Geprüft nach …“ for a procedure criterion. */
  private cardTitle(criterion: Criterion): string {
    const testType = procedureTestType(criterion);
    return testType === null
      ? requirementTitle(criterion)
      : (this.translate.instant('criteria.item.testedBy.title', {
          testType: this.translate.instant(`testType.${testType}`) as string,
        }) as string);
  }

  /**
   * „Neue Fassung laden“: the list as stored now, with the loading bar; the editor starts on it, the
   * focus goes to the H1. A list that is gone shows „Seite nicht gefunden“ in place.
   */
  private async reload(): Promise<void> {
    this.store.incrementLoadingCount();
    let view: SavedListView;
    try {
      view = await this.load.reload();
    } finally {
      this.store.decrementLoadingCount();
    }
    if (this.destroyed) return;
    this.load.view = view;
    this.show({ fromRoute: false });
    this.pageTitle.focusPageHeading();
  }

  /**
   * Shows the view of the route; `fromRoute: false` for a list read again after a conflict, which
   * reports no added criteria (the shown decisions held unsaved ones) and has no arrival report.
   */
  private show({ fromRoute }: { fromRoute: boolean } = { fromRoute: true }): void {
    if (fromRoute) this.reportAddedCriteria();
    clearTimeout(this.searchTimer);
    this.searchInput.set('');
    this.query.set('');
    this.levels.set([]);
    this.onlyIncluded.set(false);
    this.announcedFilter = undefined;
    const view = this.load.view;
    if (view.kind === 'list') {
      this.editor.start(view.list.id, view.list.etag, view.criteria.criteria, view.decisions);
    }
    this.view.set(view);
    const state = fromRoute ? (this.router.currentNavigation()?.extras.state as SavedListNavigationState | undefined) : undefined;
    const arrival = view.kind === 'list' ? (state?.savedListArrival ?? null) : null;
    this.arrival.set(arrival);
    if (arrival !== null) void this.announceArrival(arrival);
  }

  private async announceArrival(arrival: SavedListArrival): Promise<void> {
    if (arrival.kind === 'copied') {
      await this.announcer.announce('criteria.list.copy.done', { release: arrival.releaseVersion });
      if (arrival.droppedValues.length > 0) {
        await this.announcer.announce('criteria.list.copy.dropped', { values: arrival.droppedValues.join(', ') });
      }
      if (arrival.notCarried.length > 0) {
        const count = arrival.notCarried.length;
        await this.announcer.announce(pluralKey(this.translate, 'criteria.list.copy.notCarried', count), {
          count,
          criteria: this.criteriaOf(arrival.notCarried),
        });
      }
    } else if (arrival.droppedDecisions.length > 0) {
      const count = arrival.droppedDecisions.length;
      await this.announcer.announce(pluralKey(this.translate, 'criteria.list.selectionChanged.dropped', count), {
        count,
        criteria: this.criteriaOf(arrival.droppedDecisions),
      });
    }
  }

  private announceResults(): void {
    const count = this.visible().length;
    void this.announcer.announce(
      pluralKey(this.translate, RESULTS_KEY, count),
      { count },
      { throttleMs: RESULTS_THROTTLE_MS, throttleKey: RESULTS_KEY },
    );
  }
}
