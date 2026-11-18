// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import type { MockInstance } from 'vitest';

import { API_URL } from '@app/app.config';
import { FakeMediaMatcher, provideFakeMediaMatcher } from '@app/testing/fake-media-matcher';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ProcureAccessStore } from './app.store';

// S6-24: the real root store with every slice; only the backend is replaced.
const PROBE = '/api/v1/auth/session';
const PRODUCTS = '/api/Products';
const OVERRIDE_WARNING = '@ngrx/signals: SignalStore members cannot be overridden.';

let backend: HttpTestingController;
let warn: MockInstance<typeof console.warn>;

function setUp() {
  warn = vi.spyOn(console, 'warn');
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideTestTranslations(),
      provideFakeMediaMatcher(new FakeMediaMatcher()),
      { provide: API_URL, useValue: '/api' },
    ],
  });
  backend = TestBed.inject(HttpTestingController);
  return TestBed.inject(ProcureAccessStore);
}

/** Lets the store's awaited promises settle (a macrotask). */
function settle(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, 0));
}

afterEach(() => warn.mockRestore());

describe('ProcureAccessStore (root store, S6-24)', () => {
  it('adds every member once, so NgRx does not warn about overridden members in dev mode', () => {
    setUp();

    const overrides = warn.mock.calls.filter(([message]) => message === OVERRIDE_WARNING);
    expect(overrides).toEqual([]);
  });

  it('has one loading counter: a load of a slice shows in the store’s isLoading', async () => {
    const store = setUp();

    backend.expectOne(PROBE).flush({ user: null });
    await settle();
    backend.expectOne(PRODUCTS).flush({ value: [] });
    await settle();
    expect(store.isLoading()).toBe(false);

    const load = store.loadFilters();
    expect(store.isLoading()).toBe(true);
    backend.expectOne('/api/FilterTypes').flush({ value: [] });
    backend.expectOne('/api/CriteriaFilters').flush({ value: [] });
    await load;
    expect(store.isLoading()).toBe(false);
  });

  it('starts without an unhandled rejection when the product list fails (S3-02 R1)', async () => {
    const store = setUp();

    backend.expectOne(PROBE).flush({ user: null });
    await settle();
    backend.expectOne(PRODUCTS).flush(null, { status: 500, statusText: 'Server Error' });
    await settle();

    // Vitest fails the run on an unhandled rejection; the loading bar is off again.
    expect(store.isLoading()).toBe(false);
    expect(store.productsLoaded()).toBe(false);
  });

  it('forgets the projects of the workspace with the user’s other data (S6-13)', async () => {
    const store = setUp();
    backend.expectOne(PROBE).flush({ user: null });
    await settle();
    backend.expectOne(PRODUCTS).flush({ value: [] });
    await settle();

    const load = store.loadProjects('o1');
    backend
      .expectOne('/api/v1/organizations/o1/projects')
      .flush([{ id: 'p1', name: 'Projekt', status: 'open', productCount: 0, modifiedAt: '2026-11-18T09:00:00Z' }]);
    await load;
    expect(store.workspace.projects().items).toHaveLength(1);

    store.clearUserData();

    expect(store.workspace.projects()).toEqual({ organizationId: null, status: 'idle', items: [] });
  });
});
