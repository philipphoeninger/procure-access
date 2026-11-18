// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { CandidateRequest } from '../../models/candidates';
import { TEST_CANDIDATE } from '../../testing/candidate-fixtures';
import { CandidatesApiService } from './candidates-api.service';

const BODY: CandidateRequest = {
  name: 'BewerberPro',
  vendor: '',
  website: '',
  functionalSummary: '',
  contactNotes: '',
  internalNotes: '',
};

describe('CandidatesApiService', () => {
  let api: CandidatesApiService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    api = TestBed.inject(CandidatesApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('reads one candidate, every failure shown by the caller (S34)', async () => {
    const read = api.get('c/1');

    const request = backend.expectOne('/api/v1/candidates/c%2F1');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(TEST_CANDIDATE);
    await expect(read).resolves.toEqual(TEST_CANDIDATE);
  });

  it('adds a candidate to the project with every field; S33 shows field errors, antiforgery and the unconfirmed address (R-CAN-02)', async () => {
    const created = api.create('p/1', BODY);

    const request = backend.expectOne('/api/v1/projects/p%2F1/candidates');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(BODY);
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed']);
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(false);
    request.flush(TEST_CANDIDATE, { status: 201, statusText: 'Created' });
    await expect(created).resolves.toEqual(TEST_CANDIDATE);
  });

  it('changes a candidate with PATCH and every field, an empty one clearing it (R-CAN-02)', async () => {
    const changed = api.change('c/1', BODY);

    const request = backend.expectOne('/api/v1/candidates/c%2F1');
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual(BODY);
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed']);
    request.flush(TEST_CANDIDATE);
    await expect(changed).resolves.toEqual(TEST_CANDIDATE);
  });
});
