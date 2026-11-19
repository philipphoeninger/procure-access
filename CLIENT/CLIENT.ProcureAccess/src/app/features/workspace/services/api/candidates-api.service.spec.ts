// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { CandidateRequest, TestEvidence, TestEvidenceRequest } from '../../models/candidates';
import { bodyOf, TEST_ASSESSMENTS, TEST_CANDIDATE } from '../../testing/candidate-fixtures';
import { CandidatesApiService } from './candidates-api.service';

const BODY: CandidateRequest = {
  name: 'BewerberPro',
  vendor: '',
  website: '',
  functionalSummary: '',
  contactNotes: '',
  internalNotes: '',
};

const CANDIDATE_BODY = bodyOf(TEST_CANDIDATE);

const EVIDENCE_BODY: TestEvidenceRequest = {
  testKind: 'procedure',
  testTypeId: 'softwareTest',
  otherTestTypeName: null,
  testedBy: 'Prüfstelle Beispiel',
  testDate: '2026-03-14',
  testedVersion: '4.2',
  evidenceUrl: '',
  qualificationNote: '',
};

const EVIDENCE: TestEvidence = { id: 'e-1', etag: '"1-1"', ...EVIDENCE_BODY, evidenceUrl: null, qualificationNote: null };

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

  it('reads one candidate with the ETag of the answer, every failure shown by the caller (S34, API-03)', async () => {
    const read = api.get('c/1');

    const request = backend.expectOne('/api/v1/candidates/c%2F1');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(CANDIDATE_BODY, { headers: { ETag: '"1-7"' } });
    await expect(read).resolves.toEqual({ ...TEST_CANDIDATE, etag: '"1-7"' });
  });

  it('adds a candidate to the project with every field; S33 shows field errors, antiforgery and the unconfirmed address (R-CAN-02)', async () => {
    const created = api.create('p/1', BODY);

    const request = backend.expectOne('/api/v1/projects/p%2F1/candidates');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(BODY);
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed']);
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(false);
    request.flush(CANDIDATE_BODY, { status: 201, statusText: 'Created', headers: { ETag: TEST_CANDIDATE.etag } });
    await expect(created).resolves.toEqual(TEST_CANDIDATE);
  });

  it('changes a candidate with PATCH, every field and the tag it was read with in If-Match, and keeps the new tag (R-CAN-02, API-03)', async () => {
    const changed = api.change('c/1', BODY, '"1-4"');

    const request = backend.expectOne('/api/v1/candidates/c%2F1');
    expect(request.request.method).toBe('PATCH');
    expect(request.request.body).toEqual(BODY);
    expect(request.request.headers.get('If-Match')).toBe('"1-4"');
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed']);
    request.flush(CANDIDATE_BODY, { headers: { ETag: '"1-5"' } });
    await expect(changed).resolves.toEqual({ ...TEST_CANDIDATE, etag: '"1-5"' });
  });

  it('sets the test status with PUT and the candidate’s tag in If-Match, and keeps the new tag; S34a also handles the conflict of leaving „Ja“ itself (R-CAN-05, WSP-01, API-03)', async () => {
    const changed = api.changeTestStatus('c/1', 'notTested', true, '"1-4"');

    const request = backend.expectOne('/api/v1/candidates/c%2F1/test-status');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ testStatus: 'notTested', removeEvidences: true });
    expect(request.request.headers.get('If-Match')).toBe('"1-4"');
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual([
      'validation',
      'antiforgery',
      'email-unconfirmed',
      'test-status-conflict',
    ]);
    request.flush({ ...CANDIDATE_BODY, testStatus: 'notTested' }, { headers: { ETag: '"1-5"' } });
    await expect(changed).resolves.toEqual({ ...TEST_CANDIDATE, testStatus: 'notTested', etag: '"1-5"' });
  });

  it('adds an evidence without a tag, changes and removes it with its own tag in If-Match; the card shows field errors, antiforgery and the unconfirmed address (R-CAN-05, API-03)', async () => {
    const added = api.addEvidence('c/1', EVIDENCE_BODY);
    const post = backend.expectOne('/api/v1/candidates/c%2F1/test-evidences');
    expect(post.request.method).toBe('POST');
    expect(post.request.headers.has('If-Match')).toBe(false);
    expect(post.request.body).toEqual(EVIDENCE_BODY);
    expect(post.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed']);
    post.flush(EVIDENCE, { status: 201, statusText: 'Created' });
    await expect(added).resolves.toEqual(EVIDENCE);

    const changed = api.changeEvidence('e/1', EVIDENCE_BODY, '"1-1"');
    const put = backend.expectOne('/api/v1/test-evidences/e%2F1');
    expect(put.request.method).toBe('PUT');
    expect(put.request.headers.get('If-Match')).toBe('"1-1"');
    expect(put.request.body).toEqual(EVIDENCE_BODY);
    put.flush(EVIDENCE);
    await expect(changed).resolves.toEqual(EVIDENCE);

    const removed = api.removeEvidence('e/1', '"1-2"');
    const remove = backend.expectOne('/api/v1/test-evidences/e%2F1');
    expect(remove.request.method).toBe('DELETE');
    expect(remove.request.headers.get('If-Match')).toBe('"1-2"');
    expect(remove.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['validation', 'antiforgery', 'email-unconfirmed', 'not-found']);
    remove.flush(null, { status: 204, statusText: 'No Content' });
    await expect(removed).resolves.toBeNull();
  });

  it('reads the assessment tab of a candidate, every failure shown by the caller (S34b, R-CAN-06)', async () => {
    const read = api.assessments('c/1');

    const request = backend.expectOne('/api/v1/candidates/c%2F1/assessments');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(TEST_ASSESSMENTS);
    await expect(read).resolves.toEqual(TEST_ASSESSMENTS);
  });

  it('saves the whole row of a criterion with PUT and without If-Match; a blank note is sent as none; failures keep the system states (R-CAN-06, R-CAN-11)', async () => {
    const saved = api.saveAssessment('c/1', 12, { value: 'barrier', note: '  ' });

    const request = backend.expectOne('/api/v1/candidates/c%2F1/assessments/12');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ value: 'barrier', note: null });
    expect(request.request.headers.has('If-Match')).toBe(false);
    expect(request.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual([]);
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(false);
    request.flush({ criterionRevisionId: 12, value: 'barrier', note: null });
    await expect(saved).resolves.toBeUndefined();

    // A note without a value is stored too (it is no assessment, SPEC v2.56-2)
    void api.saveAssessment('c-1', 11, { value: 'unknown', note: 'Laut Anbieter.' });
    const noteOnly = backend.expectOne('/api/v1/candidates/c-1/assessments/11');
    expect(noteOnly.request.body).toEqual({ value: 'unknown', note: 'Laut Anbieter.' });
    noteOnly.flush({ criterionRevisionId: 11, value: 'unknown', note: 'Laut Anbieter.' });
  });
});
