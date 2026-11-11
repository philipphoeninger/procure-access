// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore, signalStoreFeature, withMethods } from '@ngrx/signals';

import { withLoading } from './with-loading';

// Two features that add withLoading() again, as the legacy features and withConfigurator() do.
const legacyFeature = () =>
  signalStoreFeature(
    withLoading(),
    withMethods((store) => ({
      legacyStart: () => store.incrementLoadingCount(),
      legacyEnd: () => store.decrementLoadingCount(),
    })),
  );
const configuratorFeature = () =>
  signalStoreFeature(
    withLoading(),
    withMethods((store) => ({
      configuratorStart: () => store.incrementLoadingCount(),
      configuratorEnd: () => store.decrementLoadingCount(),
    })),
  );
const RootStore = signalStore(withLoading(), legacyFeature(), configuratorFeature());

describe('withLoading', () => {
  let store: InstanceType<typeof RootStore>;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [RootStore] });
    store = TestBed.inject(RootStore);
  });

  it('shares one counter between every feature that adds it', () => {
    store.legacyStart();
    store.legacyEnd();
    expect(store.loadingCount()).toBe(0);

    store.configuratorStart();
    expect(store.isLoading()).toBe(true);

    store.legacyStart();
    store.configuratorEnd();
    expect(store.isLoading()).toBe(true);

    store.legacyEnd();
    expect(store.isLoading()).toBe(false);
    expect(store.loadingCount()).toBe(0);
  });
});
