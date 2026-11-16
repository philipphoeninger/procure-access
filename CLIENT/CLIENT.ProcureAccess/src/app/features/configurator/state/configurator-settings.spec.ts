// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_URL } from '@app/app.config';

import { ConfiguratorSettings } from './configurator-settings';

describe('ConfiguratorSettings (SPEC §8.11, TD-45)', () => {
  let settings: ConfiguratorSettings;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    settings = TestBed.inject(ConfiguratorSettings);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('reads the weight range from the server once, for every caller', async () => {
    const first = settings.ready();
    const second = settings.ready();
    backend.expectOne({ method: 'GET', url: '/api/v1/configurator/settings' }).flush({ priorityWeight: { min: 2, max: 7 } });

    expect(await first).toBe(true);
    expect(await second).toBe(true);
    expect(settings.weightRange()).toEqual({ min: 2, max: 7 });
    expect(await settings.ready()).toBe(true);
  });

  it('answers false on a failure and asks again next time', async () => {
    const failed = settings.ready();
    backend.expectOne('/api/v1/configurator/settings').flush(null, { status: 500, statusText: 'Server Error' });
    expect(await failed).toBe(false);

    const retried = settings.ready();
    backend.expectOne('/api/v1/configurator/settings').flush({ priorityWeight: { min: 1, max: 10 } });
    expect(await retried).toBe(true);
  });

  it('refuses to answer a range it has not loaded', () => {
    expect(() => settings.weightRange()).toThrowError(/before ready\(\) resolved/);
  });
});
