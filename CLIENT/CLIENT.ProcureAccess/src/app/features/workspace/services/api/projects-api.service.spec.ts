// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { API_URL } from '@app/app.config';
import { CALLER_SHOWS_FAILURES } from '@app/core/errors/problem-details';

import { Project } from '../../models/projects';
import { bodyOf } from '../../testing/candidate-fixtures';
import { ProjectsApiService } from './projects-api.service';

const PROJECT: Project = {
  id: 'p1',
  organizationId: 'o/1',
  etag: '"1-1"',
  name: 'Bewerbermanagement',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
};

const BODY = bodyOf(PROJECT);

describe('ProjectsApiService', () => {
  let api: ProjectsApiService;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    api = TestBed.inject(ProjectsApiService);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('lists the projects of the organisation named in the route, every failure shown by the caller (R-PRJ-01)', async () => {
    const listed = api.list('o/1');

    const request = backend.expectOne('/api/v1/organizations/o%2F1/projects');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush([PROJECT]);
    await expect(listed).resolves.toEqual([PROJECT]);
  });

  it('creates a project with its name only and resolves to the created project (R-PRJ-02)', async () => {
    const created = api.create('o/1', 'Bewerbermanagement');

    const request = backend.expectOne('/api/v1/organizations/o%2F1/projects');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({ name: 'Bewerbermanagement' });
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(BODY, { status: 201, statusText: 'Created', headers: { ETag: PROJECT.etag } });
    await expect(created).resolves.toEqual(PROJECT);
  });

  it('reads one project with the ETag of the answer, every failure shown by the caller (API-03)', async () => {
    const read = api.get('p1');

    const request = backend.expectOne('/api/v1/projects/p1');
    expect(request.request.method).toBe('GET');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    request.flush(BODY, { headers: { ETag: '"1-7"' } });
    await expect(read).resolves.toEqual({ ...PROJECT, etag: '"1-7"' });
  });

  it("gives the project its own copy of a list with a PUT of the source and the project's tag in If-Match, every failure shown by the caller (R-PRJ-02, R-PRJ-04, API-03)", async () => {
    const assigned = api.assignCriteriaList('p/1', 'list-1', '"1-4"');

    const request = backend.expectOne('/api/v1/projects/p%2F1/criteria-configuration');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({ sourceConfigurationId: 'list-1' });
    expect(request.request.headers.get('If-Match')).toBe('"1-4"');
    expect(request.request.context.get(CALLER_SHOWS_FAILURES)).toBe(true);
    const copy = { id: 'copy-1', name: 'HR-Software 2027', releaseVersion: 'T.1' };
    request.flush(copy);
    await expect(assigned).resolves.toEqual(copy);
  });
});
