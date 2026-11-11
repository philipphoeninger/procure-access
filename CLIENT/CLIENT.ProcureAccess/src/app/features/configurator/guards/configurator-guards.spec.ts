// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, provideRouter, Router, RouterStateSnapshot, UrlTree } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { FilterOptionsStatus } from '../state/with-configurator';
import { completeSelectionGuard, configuratorReleaseGuard } from './configurator-guards';

describe('configurator guards', () => {
  const complete = signal(false);
  let status: FilterOptionsStatus;

  beforeEach(() => {
    complete.set(false);
    status = 'loaded';
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: ProcureAccessStore,
          useValue: {
            filterOptionsReady: () => Promise.resolve(status),
            configuratorComplete: complete,
          },
        },
      ],
    });
  });

  const route = {} as ActivatedRouteSnapshot;
  const state = {} as RouterStateSnapshot;
  const run = (guard: typeof configuratorReleaseGuard) =>
    TestBed.runInInjectionContext(() => guard(route, state)) as Promise<boolean | UrlTree> | boolean | UrlTree;
  const url = (result: boolean | UrlTree) => (result instanceof UrlTree ? TestBed.inject(Router).serializeUrl(result) : result);

  describe('S10 or the legacy filter (O-S4-3)', () => {
    it('opens S10 when a release is active', async () => {
      expect(url(await run(configuratorReleaseGuard))).toBe(true);
    });

    it('opens the legacy filter instead when no release is active (404)', async () => {
      status = 'noRelease';

      expect(url(await run(configuratorReleaseGuard))).toBe('/filters');
    });

    it('opens S10 after any other failure, where the system state explains it', async () => {
      status = 'failed';

      expect(url(await run(configuratorReleaseGuard))).toBe(true);
    });
  });

  describe('S11 needs a complete selection (R-CON-10)', () => {
    it('opens S11 for a complete selection', async () => {
      complete.set(true);

      expect(url(await run(completeSelectionGuard))).toBe(true);
    });

    it('leads back to S10 when the selection is gone, e.g. after a reload (R-CON-09)', async () => {
      expect(url(await run(completeSelectionGuard))).toBe('/kriterien');
    });
  });
});
