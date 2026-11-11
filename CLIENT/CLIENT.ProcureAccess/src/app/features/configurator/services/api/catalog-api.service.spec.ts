// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS } from '@app/core/errors/problem-details';

import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { CatalogApiService } from './catalog-api.service';

const url = '/api/v1/catalog/filter-options';

describe('CatalogApiService', () => {
  let api: CatalogApiService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: API_URL, useValue: '/api' },
      ],
    });
    api = TestBed.inject(CatalogApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('asks for the filter options of the active release without a selection', async () => {
    const answer = api.getFilterOptions({ appTypeIds: [], productPartIds: [] });

    const request = backend.expectOne((r) => r.url === url);
    expect(request.request.method).toBe('GET');
    expect(request.request.params.keys()).toEqual([]);
    request.flush(filterOptionsAnswer());

    expect(await answer).toEqual(filterOptionsAnswer());
  });

  it('sends the release and the selection under the parameter names of SPEC §8.1', async () => {
    const answer = api.getFilterOptions(
      { useCase: 'hr', appTypeIds: ['web', 'closed'], productPartIds: ['base', 'audio'] },
      'T.1',
    );

    const request = backend.expectOne((r) => r.url === url);
    const params = request.request.params;
    expect(params.get('release')).toBe('T.1');
    expect(params.get('useCase')).toBe('hr');
    expect(params.getAll('appTypeIds[]')).toEqual(['web', 'closed']);
    expect(params.getAll('productPartIds[]')).toEqual(['base', 'audio']);
    expect(params.keys()).toEqual(['release', 'useCase', 'appTypeIds[]', 'productPartIds[]']);
    request.flush(filterOptionsAnswer());

    await answer;
  });

  it('rejects with the HTTP error of a Problem Details answer', async () => {
    const answer = api.getFilterOptions({ appTypeIds: [], productPartIds: [] });

    backend
      .expectOne((r) => r.url === url)
      .flush({ title: 'Not Found' }, { status: 404, statusText: 'Not Found' });

    await expect(answer).rejects.toMatchObject({ status: 404 });
  });

  it('handles a 404 of the filter options itself, so the interceptor shows no snackbar (O-S4-3)', async () => {
    const answer = api.getFilterOptions({ appTypeIds: [], productPartIds: [] });

    const request = backend.expectOne((r) => r.url === url);
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['not-found']);
    request.flush(filterOptionsAnswer());

    await answer;
  });

  describe('getCriteria (SPEC §8.2)', () => {
    const criteriaUrl = '/api/v1/catalog/criteria';
    const criteriaAnswer = { release: 'T.1', criteria: [], appTypesWithoutCriteria: [] };

    it('sends the release, the selection and the test types', async () => {
      const answer = api.getCriteria(
        { useCase: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: ['softwareTest', 'pdfTest'] },
        'T.1',
      );

      const request = backend.expectOne((r) => r.url === criteriaUrl);
      const params = request.request.params;
      expect(request.request.method).toBe('GET');
      expect(params.keys()).toEqual(['release', 'useCase', 'appTypeIds[]', 'productPartIds[]', 'testTypeIds[]']);
      expect(params.getAll('testTypeIds[]')).toEqual(['softwareTest', 'pdfTest']);
      request.flush(criteriaAnswer);

      expect(await answer).toEqual(criteriaAnswer);
    });

    it('shows the server check of the selection itself: no snackbar for validation, selection-excluded, not-found', async () => {
      const answer = api.getCriteria({ appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] });

      const request = backend.expectOne((r) => r.url === criteriaUrl);
      expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual([
        'validation',
        'selection-excluded',
        'not-found',
      ]);
      expect(request.request.params.has('useCase')).toBe(false);
      request.flush(criteriaAnswer);

      await answer;
    });
  });
});
