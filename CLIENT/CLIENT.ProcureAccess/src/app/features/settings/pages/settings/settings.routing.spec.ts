// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { SettingsLoad } from './settings-load';
import { settingsResolver } from './settings.routing';

describe('settingsResolver (S60)', () => {
  let readPreferences: ReturnType<typeof vi.fn>;
  let incrementLoadingCount: ReturnType<typeof vi.fn>;
  let decrementLoadingCount: ReturnType<typeof vi.fn>;

  function resolve(): Promise<string> {
    TestBed.configureTestingModule({
      providers: [SettingsLoad, { provide: ProcureAccessStore, useValue: { readPreferences, incrementLoadingCount, decrementLoadingCount } }],
    });
    return TestBed.runInInjectionContext(
      () => settingsResolver({} as ActivatedRouteSnapshot, {} as RouterStateSnapshot) as Promise<string>,
    );
  }

  beforeEach(() => {
    incrementLoadingCount = vi.fn();
    decrementLoadingCount = vi.fn();
  });

  it('reads the settings with the loading bar and titles the page', async () => {
    readPreferences = vi.fn().mockResolvedValue(undefined);

    await expect(resolve()).resolves.toBe('pageTitle.settings');

    expect(readPreferences).toHaveBeenCalledTimes(1);
    expect(TestBed.inject(SettingsLoad).failed).toBe(false);
    expect(incrementLoadingCount).toHaveBeenCalledTimes(1);
    expect(decrementLoadingCount).toHaveBeenCalledTimes(1);
  });

  it('a failed read titles the S80 server text and tells the page', async () => {
    readPreferences = vi.fn().mockRejectedValue(new Error('request failed'));

    await expect(resolve()).resolves.toBe('pageTitle.server');

    expect(TestBed.inject(SettingsLoad).failed).toBe(true);
    expect(decrementLoadingCount).toHaveBeenCalledTimes(1);
  });
});
