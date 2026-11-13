// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ApplicationInitStatus } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';

import { provideSessionProbe, SessionProbeService } from './session-probe.service';

const PROBE = '/api/v1/auth/session';

describe('SessionProbeService', () => {
  let api: SessionProbeService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    api = TestBed.inject(SessionProbeService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('calls the probe with a GET marked as a background request (SPEC §8.8)', async () => {
    const probe = api.probe();

    const request = backend.expectOne(PROBE);
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(true);
    request.flush({ user: null });
    await expect(probe).resolves.toBeUndefined();
  });

  it('resolves also when the probe fails, so the app start never depends on it', async () => {
    const probe = api.probe();

    backend.expectOne(PROBE).flush(null, { status: 500, statusText: 'Internal Server Error' });

    await expect(probe).resolves.toBeUndefined();
  });
});

describe('provideSessionProbe', () => {
  it('calls the probe once at application start (ARCHITECTURE §8.3)', async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }, provideSessionProbe()],
    });

    await TestBed.inject(ApplicationInitStatus).donePromise;

    const backend = TestBed.inject(HttpTestingController);
    backend.expectOne(PROBE).flush({ user: null });
    backend.verify();
  });
});
