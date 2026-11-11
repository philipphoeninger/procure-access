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
import { signalStore } from '@ngrx/signals';

import { FilterOptionsLayers } from '../models/filter-options';
import { excluded, filterOptions, filterOptionsAnswer } from '../testing/filter-options-fixture';
import { withConfigurator } from './with-configurator';

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

  afterEach(() => backend.verify());

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
        store.chooseUseCase({ kind: 'none' });
        await answer();

        expect(store.configurator.draft.useCase()).toEqual({ kind: 'none' });
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
