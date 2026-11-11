// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST } from '@app/core/errors/problem-details';

import { ConfiguratorApiService } from './configurator-api.service';

describe('ConfiguratorApiService', () => {
  let api: ConfiguratorApiService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_URL, useValue: '/api' },
      ],
    });
    api = TestBed.inject(ConfiguratorApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('reports a shown list with a POST without body, marked as a background request (SPEC §8.7)', async () => {
    const report = api.reportCriteriaListShown();

    const request = backend.expectOne('/api/v1/configurator/criteria-list-shown');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    expect(request.request.context.get(BACKGROUND_REQUEST)).toBe(true);
    request.flush(null, { status: 204, statusText: 'No Content' });
    await expect(report).resolves.toBeNull();
  });
});
