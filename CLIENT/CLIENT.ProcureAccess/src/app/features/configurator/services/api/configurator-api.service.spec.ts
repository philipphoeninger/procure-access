// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_HANDLES_PROBLEMS, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

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

  it('lists, creates and revokes the share links of a list, every failure shown by the caller (S15)', async () => {
    const listed = api.shareLinks('l1');
    const list = backend.expectOne('/api/v1/configurations/l1/share-links');
    expect(list.request.method).toBe('GET');
    expect(list.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    const answer = { expiry: { earliest: '2026-11-17', default: '2027-02-14', latest: '2027-11-16' }, links: [] };
    list.flush(answer);
    await expect(listed).resolves.toEqual(answer);

    const created = api.createShareLink('l1', '2027-02-14');
    const create = backend.expectOne('/api/v1/configurations/l1/share-links');
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual({ expiresOn: '2027-02-14' });
    expect(create.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    create.flush({ id: 's1', token: 't', expiresAt: '2027-02-14T23:00:00Z' }, { status: 201, statusText: 'Created' });
    await expect(created).resolves.toEqual({ id: 's1', token: 't', expiresAt: '2027-02-14T23:00:00Z' });

    const revoked = api.revokeShareLink('s1');
    const revoke = backend.expectOne('/api/v1/share-links/s1');
    expect(revoke.request.method).toBe('DELETE');
    expect(revoke.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    revoke.flush(null, { status: 204, statusText: 'No Content' });
    await revoked;
  });

  it('INV:API-03 reads a saved list with the ETag of the answer and sends it back with a decision, a selection change and a deletion', async () => {
    const read = api.get('l1');
    const get = backend.expectOne('/api/v1/configurations/l1');
    expect(get.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    get.flush({ id: 'l1', name: 'Liste' }, { headers: { ETag: '"1-4"' } });
    await expect(read).resolves.toEqual({ id: 'l1', name: 'Liste', etag: '"1-4"' });

    const decision = { isIncluded: false, overrideLevel: null, overrideWeight: null, note: null };
    const decided = api.decide('l1', 11, decision, '"1-4"');
    const patch = backend.expectOne('/api/v1/configurations/l1/criteria/11');
    expect(patch.request.method).toBe('PATCH');
    expect(patch.request.body).toEqual(decision);
    expect(patch.request.headers.get('If-Match')).toBe('"1-4"');
    // A conflict opens S37 on S11, not the general error (S6-21b)
    expect(patch.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['conflict']);
    patch.flush(null, { status: 204, statusText: 'No Content', headers: { ETag: '"1-5"' } });
    // The new tag of the list, for the next change.
    await expect(decided).resolves.toBe('"1-5"');

    const selection = { selection: { releaseVersion: 'T.1', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] } };
    const changed = api.changeSelection('l1', selection, '"1-5"');
    const put = backend.expectOne('/api/v1/configurations/l1/selection');
    expect(put.request.method).toBe('PUT');
    expect(put.request.headers.get('If-Match')).toBe('"1-5"');
    expect(put.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual(['conflict']);
    put.flush({ droppedDecisions: [] });
    await expect(changed).resolves.toEqual({ droppedDecisions: [] });

    const deleted = api.delete('l1', '"1-6"');
    const remove = backend.expectOne('/api/v1/configurations/l1');
    expect(remove.request.method).toBe('DELETE');
    expect(remove.request.headers.get('If-Match')).toBe('"1-6"');
    // S14 keeps the general error for a conflict (S6-21a open point 2)
    expect(remove.request.context.get(CALLER_HANDLES_PROBLEMS)).toEqual([]);
    remove.flush(null, { status: 204, statusText: 'No Content' });
    await deleted;
  });

  it('INV:API-03 refuses the answer of a decision without an ETag: the next change could not name its version', async () => {
    const decided = api.decide('l1', 11, { isIncluded: true, overrideLevel: null, overrideWeight: null, note: null }, '"1-4"');
    backend.expectOne('/api/v1/configurations/l1/criteria/11').flush(null, { status: 204, statusText: 'No Content' });

    await expect(decided).rejects.toThrowError(/no ETag/);
  });

  it('reads a shared list by its token, its failures shown by S16 (SPEC §8.5)', async () => {
    const read = api.shared('abc_-1');

    const request = backend.expectOne('/api/v1/shared/abc_-1');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(null, { status: 404, statusText: 'Not Found' });
    await expect(read).rejects.toMatchObject({ status: 404 });
  });
});
