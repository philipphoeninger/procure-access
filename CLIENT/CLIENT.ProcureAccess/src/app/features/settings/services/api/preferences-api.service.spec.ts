// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';
import { UserPreferences } from '../../models/user-preferences';
import { PreferencesApiService } from './preferences-api.service';

describe('PreferencesApiService (SPEC §8.12)', () => {
  let api: PreferencesApiService;
  let backend: HttpTestingController;
  const stored: UserPreferences = { language: 'de', appearance: 'dark', motion: 'reduced', contrast: 'system', assessmentLegend: 'collapsed' };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    api = TestBed.inject(PreferencesApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('reads after the sign-in as a background request', async () => {
    const answer = api.get();

    const request = backend.expectOne('/api/v1/account/preferences');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(true);
    request.flush(stored);

    await expect(answer).resolves.toEqual(stored);
  });

  it('reads for S60 as a request whose page shows failures', async () => {
    const answer = api.read();

    const request = backend.expectOne('/api/v1/account/preferences');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(false);
    request.flush(stored);

    await expect(answer).resolves.toEqual(stored);
  });

  it('saves all four with PUT, unmarked, so the error interceptor shows a failure (D2 (a))', async () => {
    const answer = api.save(stored);

    const request = backend.expectOne('/api/v1/account/preferences');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(stored);
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(false);
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(false);
    request.flush(stored);

    await expect(answer).resolves.toEqual(stored);
  });

  it('saves in the background for S34b remembering its legend, so a failure shows nothing (S6-19b)', async () => {
    const answer = api.save(stored, { background: true });

    const request = backend.expectOne('/api/v1/account/preferences');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(stored);
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(true);
    request.flush(stored);

    await expect(answer).resolves.toEqual(stored);
  });
});
