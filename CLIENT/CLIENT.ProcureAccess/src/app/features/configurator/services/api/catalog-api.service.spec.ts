// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';

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
});
