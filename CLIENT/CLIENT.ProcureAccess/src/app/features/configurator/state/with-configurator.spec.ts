// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
  TestRequest,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS } from '@app/core/errors/problem-details';
import { signalStore } from '@ngrx/signals';

import { CriteriaResponse } from '../models/criteria';
import { FilterOptionsLayers } from '../models/filter-options';
import { criteriaAnswer } from '../testing/criteria-fixture';
import { excluded, filterOptions, filterOptionsAnswer } from '../testing/filter-options-fixture';
import { PRIORITY_WEIGHT_RANGE } from './priority-weight-range';
import { CRITERIA_SEARCH_DEBOUNCE_MS, withConfigurator } from './with-configurator';

const ConfiguratorTestStore = signalStore(withConfigurator());
const url = '/api/v1/catalog/filter-options';

describe('withConfigurator', () => {
  let store: InstanceType<typeof ConfiguratorTestStore>;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ConfiguratorTestStore,
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_URL, useValue: '/api' },
      ],
    });
    store = TestBed.inject(ConfiguratorTestStore);
    backend = TestBed.inject(HttpTestingController);
  });

  /** Reports of a shown list (S4-18b); taken out before `verify`, so only their own tests count them. */
  const shownReports = () => backend.match((r) => r.url === '/api/v1/configurator/criteria-list-shown');

  afterEach(() => {
    shownReports();
    backend.verify();
  });

  /** The next filter-options request; fails when there is none or more than one. */
  function nextRequest(): TestRequest {
    return backend.expectOne((r) => r.url === url);
  }

  /** Lets the promise of a flushed request reach the store. */
  function settle(): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve));
  }

  async function answer(layers: FilterOptionsLayers = filterOptions(), release = 'T.1') {
    nextRequest().flush(filterOptionsAnswer(layers, release));
    await settle();
  }

  async function loaded(layers: FilterOptionsLayers = filterOptions()) {
    const load = store.loadFilterOptions();
    nextRequest().flush(filterOptionsAnswer(layers));
    await load;
  }

  /** Fills all four layers, answering the reload after each change of steps 1–3. */
  async function fillAllLayers() {
    await loaded();
    store.chooseUseCase({ kind: 'useCase', id: 'hr' });
    await answer();
    store.toggleAppType('web');
    await answer();
    store.toggleProductPart('audio');
    await answer();
    store.toggleTestType('softwareTest');
  }

  describe('loading the filter options', () => {
    it('starts empty and loads the options of the active release', async () => {
      expect(store.configurator.optionsStatus()).toBe('notLoaded');

      const load = store.loadFilterOptions();
      expect(store.configurator.optionsStatus()).toBe('loading');
      expect(store.isLoading()).toBe(true);
      const request = nextRequest();
      expect(request.request.params.has('release')).toBe(false);
      request.flush(filterOptionsAnswer());
      await load;

      expect(store.configurator.optionsStatus()).toBe('loaded');
      expect(store.configurator.options()).toEqual(filterOptions());
      expect(store.isLoading()).toBe(false);
    });

    it('selects Basisfunktionalität with the first answer (R-CON-04)', async () => {
      await loaded();

      expect(store.configurator.draft.productPartIds()).toEqual(['base']);
    });

    it('pins the release of the first answer and names it in every later request (ADR-002)', async () => {
      await loaded();
      expect(store.configurator.release()).toBe('T.1');

      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      const request = nextRequest();
      expect(request.request.params.get('release')).toBe('T.1');
      request.flush(filterOptionsAnswer(filterOptions(), 'T.1'));
      await settle();
    });

    it('sends the draft with each reload after a change of steps 1–3', async () => {
      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      await answer();

      store.toggleAppType('web');
      const params = nextRequest().request.params;

      expect(params.get('useCase')).toBe('hr');
      expect(params.getAll('appTypeIds[]')).toEqual(['web']);
      expect(params.getAll('productPartIds[]')).toEqual(['base']);
    });

    it('does not reload for a test type, the lowest layer', async () => {
      await fillAllLayers();

      backend.expectNone((r) => r.url === url);
      expect(store.configurator.draft.testTypeIds()).toEqual(['softwareTest']);
    });

    it('applies only the answer of the latest request', async () => {
      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      store.chooseUseCase({ kind: 'useCase', id: 'citizen' });
      const [older, latest] = backend.match((r) => r.url === url);
      const citizenAnswer = excluded(filterOptions(), 'appTypes', 'document', {
        layer: 'useCase',
        id: 'citizen',
        name: 'Citizen',
      });

      latest.flush(filterOptionsAnswer(citizenAnswer));
      older.flush(filterOptionsAnswer());
      await settle();

      expect(store.configurator.options()).toEqual(citizenAnswer);
      expect(store.configurator.optionsStatus()).toBe('loaded');
      expect(store.isLoading()).toBe(false);
    });

    it('reports a missing release as noRelease (404)', async () => {
      const load = store.loadFilterOptions();
      nextRequest().flush({ title: 'Not Found' }, { status: 404, statusText: 'Not Found' });
      await load;

      expect(store.configurator.optionsStatus()).toBe('noRelease');
      expect(store.isLoading()).toBe(false);
    });

    it('reports any other failure as failed and keeps the earlier options for a retry', async () => {
      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      nextRequest().flush(null, { status: 500, statusText: 'Server Error' });
      await settle();

      expect(store.configurator.optionsStatus()).toBe('failed');
      expect(store.configurator.options()).toEqual(filterOptions());
      expect(store.isLoading()).toBe(false);

      const retry = store.loadFilterOptions();
      nextRequest().flush(filterOptionsAnswer());
      await retry;
      expect(store.configurator.optionsStatus()).toBe('loaded');
    });
  });

  describe('selection', () => {
    it('ignores every choice before the options arrived', () => {
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });

      expect(store.configurator.draft.useCase()).toEqual({ kind: 'unanswered' });
      backend.expectNone((r) => r.url === url);
    });

    it('holds one use case or „Keiner davon passt“ and tells when the selection is complete', async () => {
      await loaded();
      expect(store.configuratorComplete()).toBe(false);

      store.chooseUseCase({ kind: 'none' });
      const request = nextRequest();
      expect(request.request.params.has('useCase')).toBe(false);
      request.flush(filterOptionsAnswer());
      await settle();
      store.toggleAppType('web');
      await answer();

      expect(store.configurator.draft.useCase()).toEqual({ kind: 'none' });
      expect(store.configuratorComplete()).toBe(true);
    });

    it('changes nothing when an excluded option is chosen (R-CON-06)', async () => {
      await loaded(
        excluded(filterOptions(), 'appTypes', 'document', { layer: 'useCase', id: 'hr', name: 'HR' }),
      );

      store.toggleAppType('document');

      expect(store.configurator.draft.appTypeIds()).toEqual([]);
      backend.expectNone((r) => r.url === url);
    });

    it('waits with choices below a changed layer until its options arrived', async () => {
      await fillAllLayers();
      store.toggleAppType('closed');

      store.toggleProductPart('video');
      store.toggleTestType('pdfTest');
      expect(store.configurator.draft.productPartIds()).toEqual(['base']);
      expect(store.configurator.draft.testTypeIds()).toEqual([]);

      await answer();
      store.toggleProductPart('video');
      expect(store.configurator.draft.productPartIds()).toEqual(['base', 'video']);
      await answer();
    });

    it('keeps choices below a changed layer blocked after a failed reload', async () => {
      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      nextRequest().flush(null, { status: 500, statusText: 'Server Error' });
      await settle();

      store.toggleAppType('web');

      expect(store.configurator.draft.appTypeIds()).toEqual([]);
    });
  });

  describe('waiting for current options („Weiter“, R-CON-01)', () => {
    it('answers at once when the options match the draft', async () => {
      await loaded();

      await expect(store.optionsCurrent()).resolves.toBe(true);
    });

    it('waits for a running reload, also for one started during the wait', async () => {
      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      let current: boolean | undefined;
      const waiting = store.optionsCurrent().then((value) => (current = value));

      store.chooseUseCase({ kind: 'useCase', id: 'citizen' });
      const [first, second] = backend.match((r) => r.url === url);
      first.flush(filterOptionsAnswer());
      await settle();
      expect(current).toBeUndefined();

      second.flush(filterOptionsAnswer());
      await waiting;
      expect(current).toBe(true);
    });

    it('answers false when the reload failed or nothing was loaded', async () => {
      await expect(store.optionsCurrent()).resolves.toBe(false);

      await loaded();
      store.chooseUseCase({ kind: 'useCase', id: 'hr' });
      const waiting = store.optionsCurrent();
      nextRequest().flush(null, { status: 500, statusText: 'Server Error' });

      await expect(waiting).resolves.toBe(false);
    });
  });

  describe('reset of the lower layers (R-CON-07)', () => {
    it('clears steps 2–4 except Basisfunktionalität and reports the cleared layers', async () => {
      await fillAllLayers();

      store.chooseUseCase({ kind: 'useCase', id: 'citizen' });

      expect(store.configurator.draft()).toEqual({
        useCase: { kind: 'useCase', id: 'citizen' },
        appTypeIds: [],
        productPartIds: ['base'],
        testTypeIds: [],
      });
      expect(store.configurator.reset()).toEqual({
        changedLayer: 'useCase',
        clearedLayers: ['appType', 'productPart', 'testType'],
      });
      await answer();
    });

    it('keeps the reset report until the next selection', async () => {
      await fillAllLayers();
      store.toggleProductPart('audio');
      expect(store.configurator.reset()?.changedLayer).toBe('productPart');

      await answer();
      expect(store.configurator.reset()).not.toBeNull();

      store.toggleTestType('pdfTest');
      expect(store.configurator.reset()).toBeNull();
    });
  });

  describe('before S10 opens (O-S4-3)', () => {
    it('loads the options and reports their status', async () => {
      const ready = store.filterOptionsReady();
      await answer();

      expect(await ready).toBe('loaded');
    });

    it('reports noRelease for a 404, and asks again on the next call', async () => {
      const first = store.filterOptionsReady();
      nextRequest().flush({ type: 'x/problems/not-found' }, { status: 404, statusText: 'Not Found' });
      expect(await first).toBe('noRelease');

      const second = store.filterOptionsReady();
      await answer();
      expect(await second).toBe('loaded');
    });

    it('waits for a request that is already running and sends no second one', async () => {
      const load = store.loadFilterOptions();
      const ready = store.filterOptionsReady();
      await answer();
      await load;

      expect(await ready).toBe('loaded');
    });

    it('answers at once when the options are loaded', async () => {
      await loaded();

      expect(await store.filterOptionsReady()).toBe('loaded');
    });
  });

  describe('„Kriterienliste anzeigen“ (R-CON-10)', () => {
    const criteriaUrl = '/api/v1/catalog/criteria';
    const criteriaRequest = () => backend.expectOne((r) => r.url === criteriaUrl);

    it('sends the whole selection with the pinned release and reports acceptance', async () => {
      await fillAllLayers();

      const submit = store.submitSelection();
      const request = criteriaRequest();
      expect(request.request.params.get('release')).toBe('T.1');
      expect(request.request.params.get('useCase')).toBe('hr');
      expect(request.request.params.getAll('appTypeIds[]')).toEqual(['web']);
      expect(request.request.params.getAll('productPartIds[]')).toEqual(['base', 'audio']);
      expect(request.request.params.getAll('testTypeIds[]')).toEqual(['softwareTest']);
      request.flush({ release: 'T.1', criteria: [], appTypesWithoutCriteria: [] });

      expect(await submit).toEqual({ kind: 'accepted' });
    });

    it('reports the first field error of selection-excluded with its layer', async () => {
      await fillAllLayers();

      const submit = store.submitSelection();
      criteriaRequest().flush(
        { type: 'x/problems/selection-excluded', errors: { 'appTypeIds[0]': ['Excluded.'], useCase: ['Other.'] } },
        { status: 400, statusText: 'Bad Request' },
      );

      expect(await submit).toEqual({
        kind: 'rejected',
        problem: 'selection-excluded',
        layer: 'appType',
        message: 'Excluded.',
      });
    });

    it('reports a validation problem whose path names no layer without a layer', async () => {
      await fillAllLayers();

      const submit = store.submitSelection();
      criteriaRequest().flush(
        { type: 'x/problems/validation', errors: { release: ['Unknown.'] } },
        { status: 400, statusText: 'Bad Request' },
      );

      expect(await submit).toEqual({ kind: 'rejected', problem: 'validation', layer: null, message: 'Unknown.' });
    });

    it('reports a 404 as noRelease, so the next visit asks for the options again', async () => {
      await fillAllLayers();

      const submit = store.submitSelection();
      criteriaRequest().flush({ type: 'x/problems/not-found' }, { status: 404, statusText: 'Not Found' });

      expect(await submit).toEqual({ kind: 'noRelease' });
      expect(store.configurator.optionsStatus()).toBe('noRelease');
    });

    it('reports any other failure as failed', async () => {
      await fillAllLayers();

      const submit = store.submitSelection();
      criteriaRequest().flush(null, { status: 500, statusText: 'Server Error' });

      expect(await submit).toEqual({ kind: 'failed' });
    });
  });

  describe('criteria list of S11 (R-LST-01 – R-LST-06)', () => {
    const listUrl = '/api/v1/catalog/criteria';
    const listRequest = () => backend.expectOne((r) => r.url === listUrl);
    const visibleKeys = () => store.visibleCriteria().map((c) => c.key);

    async function listLoaded(response: CriteriaResponse = criteriaAnswer()) {
      const load = store.loadCriteriaList();
      listRequest().flush(response);
      await load;
    }

    afterEach(() => vi.useRealTimers());

    it('loads the criteria of the draft with the pinned release', async () => {
      await fillAllLayers();
      expect(store.configurator.list.status()).toBe('notLoaded');

      const load = store.loadCriteriaList();
      expect(store.configurator.list.status()).toBe('loading');
      expect(store.isLoading()).toBe(true);
      const request = listRequest();
      expect(request.request.params.get('release')).toBe('T.1');
      expect(request.request.params.getAll('testTypeIds[]')).toEqual(['softwareTest']);
      // A 400 on S11 gets the generic snackbar; only not-found is shown by the page (O-S4-34).
      expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['not-found']);
      request.flush(criteriaAnswer());

      expect(await load).toBe('loaded');
      expect(store.configurator.list.answer()).toEqual(criteriaAnswer());
      expect(visibleKeys()).toEqual(['11.1.1', '11.2.2', '11.3.3', 'PROC-softwareTest']);
      expect(store.isLoading()).toBe(false);
    });

    it('applies only the answer of the latest request', async () => {
      await fillAllLayers();
      const first = store.loadCriteriaList();
      const second = store.loadCriteriaList();
      const [older, newer] = backend.match((r) => r.url === listUrl);

      newer?.flush({ ...criteriaAnswer(), criteria: criteriaAnswer().criteria.slice(0, 1) });
      older?.flush(criteriaAnswer());
      await Promise.all([first, second]);

      expect(visibleKeys()).toEqual(['11.1.1']);
    });

    it('reports a 404 as noRelease, so the next visit of S10 asks for the options again', async () => {
      await fillAllLayers();
      const load = store.loadCriteriaList();
      listRequest().flush({ type: 'x/problems/not-found' }, { status: 404, statusText: 'Not Found' });

      expect(await load).toBe('noRelease');
      expect(store.configurator.optionsStatus()).toBe('noRelease');
      expect(store.visibleCriteria()).toEqual([]);
    });

    it('reports any other failure as failed', async () => {
      await fillAllLayers();
      const load = store.loadCriteriaList();
      listRequest().flush(null, { status: 500, statusText: 'Server Error' });

      expect(await load).toBe('failed');
      expect(store.isLoading()).toBe(false);
    });

    it('keeps the recommended level unchanged next to an override (R-LST-03)', async () => {
      await fillAllLayers();
      await listLoaded();

      store.overrideCriterionPriority('11.1.1', 'Info');

      expect(store.configurator.list.decisions()['11.1.1']?.overrideLevel).toBe('Info');
      expect(store.configurator.list.answer()?.criteria[0]?.resolvedPriority.level).toBe('A');

      store.resetCriterionPriority('11.1.1');
      expect(store.configurator.list.decisions()).toEqual({});
    });

    it('records inclusion, weight and note, and ignores a key the list does not hold', async () => {
      await fillAllLayers();
      await listLoaded();

      store.setCriterionIncluded('11.3.3', false);
      store.setCriterionWeight('11.2.2', 9);
      store.setCriterionNote('11.2.2', 'Im Pilot prüfen');
      store.setCriterionIncluded('99.9', false);

      expect(store.configurator.list.decisions()).toEqual({
        '11.3.3': { included: false, overrideLevel: null, overrideWeight: null, note: '' },
        '11.2.2': { included: true, overrideLevel: null, overrideWeight: 9, note: 'Im Pilot prüfen' },
      });
    });

    it('reads the weight range from the environment setting (🔧)', async () => {
      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [
          ConfiguratorTestStore,
          provideHttpClient(),
          provideHttpClientTesting(),
          { provide: API_URL, useValue: '/api' },
          { provide: PRIORITY_WEIGHT_RANGE, useValue: { min: 1, max: 12 } },
        ],
      });
      store = TestBed.inject(ConfiguratorTestStore);
      backend = TestBed.inject(HttpTestingController);
      await fillAllLayers();
      await listLoaded();

      store.setCriterionWeight('11.2.2', 12);

      expect(store.configurator.list.decisions()['11.2.2']?.overrideWeight).toBe(12);
    });

    it('keeps the decisions when the selection changes and applies them when a criterion returns (O-S4-24 (a))', async () => {
      await fillAllLayers();
      await listLoaded();
      store.setCriterionIncluded('PROC-softwareTest', false);
      store.setCriterionNote('11.1.1', 'Notiz');

      store.toggleTestType('softwareTest');
      await listLoaded({ ...criteriaAnswer(), criteria: criteriaAnswer().criteria.slice(0, 3) });
      expect(store.configurator.list.decisions()['PROC-softwareTest']?.included).toBe(false);

      store.chooseUseCase({ kind: 'useCase', id: 'citizen' });
      await answer();
      store.toggleAppType('web');
      await answer();
      store.toggleTestType('softwareTest');
      await listLoaded();

      store.showOnlyIncludedCriteria(true);
      expect(visibleKeys()).toEqual(['11.1.1', '11.2.2', '11.3.3']);
      expect(store.configurator.list.decisions()['11.1.1']?.note).toBe('Notiz');
    });

    it('filters by the level that applies now and by inclusion (R-LST-04, R-LST-06)', async () => {
      await fillAllLayers();
      await listLoaded();
      store.overrideCriterionPriority('11.3.3', 'B');
      store.setCriterionIncluded('11.2.2', false);

      store.filterCriteriaByPriority(['B']);
      expect(visibleKeys()).toEqual(['11.2.2', '11.3.3', 'PROC-softwareTest']);

      store.showOnlyIncludedCriteria(true);
      expect(visibleKeys()).toEqual(['11.3.3', 'PROC-softwareTest']);

      store.filterCriteriaByPriority([]);
      store.showOnlyIncludedCriteria(false);
      expect(visibleKeys()).toEqual(['11.1.1', '11.2.2', '11.3.3', 'PROC-softwareTest']);
    });

    it('applies the search 300 ms after the last keystroke (R-LST-06)', async () => {
      await fillAllLayers();
      await listLoaded();
      vi.useFakeTimers();

      store.searchCriteria('tast');
      expect(store.configurator.list.searchInput()).toBe('tast');
      vi.advanceTimersByTime(CRITERIA_SEARCH_DEBOUNCE_MS - 1);
      store.searchCriteria('tastatur');
      vi.advanceTimersByTime(CRITERIA_SEARCH_DEBOUNCE_MS - 1);
      expect(store.configurator.list.filter.query()).toBe('');
      expect(visibleKeys()).toHaveLength(4);

      vi.advanceTimersByTime(1);
      expect(store.configurator.list.filter.query()).toBe('tastatur');
      expect(visibleKeys()).toEqual(['11.2.2']);
    });

    it('clears search, levels and inclusion at once and drops a pending search', async () => {
      await fillAllLayers();
      await listLoaded();
      vi.useFakeTimers();
      store.filterCriteriaByPriority(['A']);
      store.showOnlyIncludedCriteria(true);
      store.searchCriteria('tastatur');

      store.clearCriteriaFilter();
      vi.advanceTimersByTime(CRITERIA_SEARCH_DEBOUNCE_MS);

      expect(store.configurator.list.searchInput()).toBe('');
      expect(store.configurator.list.filter()).toEqual({ query: '', levels: [], onlyIncluded: false });
      expect(visibleKeys()).toHaveLength(4);
    });

    it('keeps search and filter when S11 asks again for the same selection (O-S4-46 (a))', async () => {
      await fillAllLayers();
      await listLoaded();
      store.filterCriteriaByPriority(['A']);
      store.searchCriteria('tastatur');

      await listLoaded();

      expect(store.configurator.list.searchInput()).toBe('tastatur');
      expect(store.configurator.list.filter.levels()).toEqual(['A']);
    });

    it('clears search and filter, also a pending search, when the list is for another selection (O-S4-46 (a))', async () => {
      await fillAllLayers();
      await listLoaded();
      vi.useFakeTimers();
      store.filterCriteriaByPriority(['A']);
      store.showOnlyIncludedCriteria(true);
      store.searchCriteria('tastatur');
      store.toggleTestType('softwareTest');

      await listLoaded();
      vi.advanceTimersByTime(CRITERIA_SEARCH_DEBOUNCE_MS);

      expect(store.configurator.list.searchInput()).toBe('');
      expect(store.configurator.list.filter()).toEqual({ query: '', levels: [], onlyIncluded: false });
    });
  });

  describe('CriteriaListShown once per new selection (R-MET-01, R-CON-10, O-S4-55)', () => {
    const listRequest = () => backend.expectOne((r) => r.url === '/api/v1/catalog/criteria');

    async function listLoaded() {
      const load = store.loadCriteriaList();
      listRequest().flush(criteriaAnswer());
      expect(await load).toBe('loaded');
    }

    async function chooseUseCase(id: string) {
      store.chooseUseCase({ kind: 'useCase', id });
      await answer();
    }

    it('reports a loaded list once and not again for the same selection', async () => {
      await fillAllLayers();

      await listLoaded();
      expect(shownReports()).toHaveLength(1);

      // „Auswahl ändern“ and back to S11 without a change, or S11 shown again.
      await listLoaded();
      expect(shownReports()).toHaveLength(0);
    });

    /** A complete selection with the given use case, Web and the given product parts. */
    async function select(useCase: string, productParts: string[] = []) {
      await chooseUseCase(useCase);
      store.toggleAppType('web');
      await answer();
      for (const part of productParts) {
        store.toggleProductPart(part);
        await answer();
      }
    }

    it('reports A → B → A three times (O-S4-55 (a), not once per distinct selection)', async () => {
      await loaded();

      await select('hr', ['audio']);
      await listLoaded();
      const first = store.configurator.list.selection();
      await select('citizen');
      await listLoaded();
      await select('hr', ['audio']);
      await listLoaded();

      expect(store.configurator.list.selection()).toEqual(first);
      expect(shownReports()).toHaveLength(3);
    });

    it('reports nothing when the list fails or no release is active', async () => {
      await fillAllLayers();

      let load = store.loadCriteriaList();
      listRequest().flush(null, { status: 500, statusText: 'Server Error' });
      expect(await load).toBe('failed');
      load = store.loadCriteriaList();
      listRequest().flush({ type: 'x/problems/not-found' }, { status: 404, statusText: 'Not Found' });
      expect(await load).toBe('noRelease');

      expect(shownReports()).toHaveLength(0);
    });

    it('reports only the answer that is applied, not a late one for another selection', async () => {
      await loaded();
      await select('hr');
      const first = store.loadCriteriaList();
      await select('citizen');
      const second = store.loadCriteriaList();
      const [older, newer] = backend.match((r) => r.url === '/api/v1/catalog/criteria');

      newer?.flush(criteriaAnswer());
      await second;
      older?.flush(criteriaAnswer());
      await first;

      expect(shownReports()).toHaveLength(1);
    });

    it('keeps the list when the report fails and does not repeat it', async () => {
      await fillAllLayers();
      await listLoaded();

      const [report] = shownReports();
      report?.flush(null, { status: 503, statusText: 'Service Unavailable' });
      await settle();
      await listLoaded();

      expect(store.configurator.list.status()).toBe('loaded');
      expect(shownReports()).toHaveLength(0);
    });
  });

  describe('memory only (R-CON-09)', () => {
    it('writes nothing to browser storage and still works while storage is blocked', async () => {
      const blocked = () => {
        throw new DOMException('Storage is blocked', 'SecurityError');
      };
      const setItem = vi.spyOn(Storage.prototype, 'setItem').mockImplementation(blocked);
      const getItem = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(blocked);
      const removeItem = vi.spyOn(Storage.prototype, 'removeItem').mockImplementation(blocked);
      const clear = vi.spyOn(Storage.prototype, 'clear').mockImplementation(blocked);
      try {
        await fillAllLayers();
        const load = store.loadCriteriaList();
        backend.expectOne((r) => r.url === '/api/v1/catalog/criteria').flush(criteriaAnswer());
        await load;
        store.setCriterionNote('11.1.1', 'Notiz');
        store.overrideCriterionPriority('11.2.2', 'A');
        store.chooseUseCase({ kind: 'none' });
        await answer();

        expect(store.configurator.draft.useCase()).toEqual({ kind: 'none' });
        expect(Object.keys(store.configurator.list.decisions())).toEqual(['11.1.1', '11.2.2']);
        expect(setItem).not.toHaveBeenCalled();
        expect(getItem).not.toHaveBeenCalled();
        expect(removeItem).not.toHaveBeenCalled();
        expect(clear).not.toHaveBeenCalled();
      } finally {
        vi.restoreAllMocks();
      }
    });

    it('starts with an empty draft in a new store, as after a reload', async () => {
      await fillAllLayers();

      TestBed.resetTestingModule();
      TestBed.configureTestingModule({
        providers: [
          ConfiguratorTestStore,
          provideHttpClient(),
          provideHttpClientTesting(),
          { provide: API_URL, useValue: '/api' },
        ],
      });
      const fresh = TestBed.inject(ConfiguratorTestStore);
      backend = TestBed.inject(HttpTestingController);

      expect(fresh.configurator.draft.useCase()).toEqual({ kind: 'unanswered' });
      expect(fresh.configurator.draft.appTypeIds()).toEqual([]);
      expect(fresh.configurator.release()).toBeNull();
    });
  });
});
