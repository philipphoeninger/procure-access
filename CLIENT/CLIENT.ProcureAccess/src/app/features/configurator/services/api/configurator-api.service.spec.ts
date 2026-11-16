// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { BACKGROUND_REQUEST, CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

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

  it('reads a shared list by its token, its failures shown by S16 (SPEC §8.5)', async () => {
    const read = api.shared('abc_-1');

    const request = backend.expectOne('/api/v1/shared/abc_-1');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(null, { status: 404, statusText: 'Not Found' });
    await expect(read).rejects.toMatchObject({ status: 404 });
  });
});
