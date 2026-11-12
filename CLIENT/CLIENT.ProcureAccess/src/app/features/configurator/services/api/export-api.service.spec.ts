// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { ExportRequest } from '../../models/export';
import { ExportApiService } from './export-api.service';

describe('ExportApiService', () => {
  let api: ExportApiService;
  let backend: HttpTestingController;
  const body: ExportRequest = {
    format: 'csv',
    selection: { releaseVersion: 'T.1', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] },
    criteria: [{ criterionRevisionId: 1, isIncluded: true }],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    api = TestBed.inject(ExportApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('posts the body, asks for a Blob, marks the request as one whose caller shows failures and reads the file name', async () => {
    const file = api.exportList(body);

    const request = backend.expectOne('/api/v1/exports');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    expect(request.request.responseType).toBe('blob');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    const content = new Blob(['a;b'], { type: 'text/csv' });
    request.flush(content, {
      headers: { 'Content-Disposition': `attachment; filename*=UTF-8''ProcureAccess_Kriterien_T.1_2026-11-12.csv` },
    });

    await expect(file).resolves.toEqual({ content, fileName: 'ProcureAccess_Kriterien_T.1_2026-11-12.csv' });
  });

  it('names the file by its format when the answer names none', async () => {
    const file = api.exportList(body);

    backend.expectOne('/api/v1/exports').flush(new Blob(['a']));

    await expect(file).resolves.toMatchObject({ fileName: 'ProcureAccess_Kriterien.csv' });
  });
});
