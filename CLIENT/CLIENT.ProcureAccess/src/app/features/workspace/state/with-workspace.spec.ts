// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { API_URL } from '@app/app.config';
import { withLoading } from '@app/shared/state/with-loading';

import { Project, ProjectSummary } from '../models/projects';
import { withWorkspace } from './with-workspace';

const WorkspaceTestStore = signalStore(withLoading(), withWorkspace());

const SUMMARY: ProjectSummary = { id: 'p1', name: 'Alt', status: 'open', productCount: 2, modifiedAt: '2026-11-01T09:00:00Z' };
const CREATED: Project = {
  id: 'p2',
  organizationId: 'o1',
  etag: '"1-1"',
  name: 'Neu',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
  criteriaListLocked: false,
  candidates: [],
};

describe('withWorkspace', () => {
  let store: InstanceType<typeof WorkspaceTestStore>;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [WorkspaceTestStore, provideHttpClient(), provideHttpClientTesting(), { provide: API_URL, useValue: '/api' }],
    });
    store = TestBed.inject(WorkspaceTestStore);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => backend.verify());

  it('loads the projects of an organisation and shows the loading bar meanwhile (R-PRJ-01)', async () => {
    const loaded = store.loadProjects('o1');
    expect(store.workspace.projects()).toEqual({ organizationId: 'o1', status: 'loading', items: [] });
    expect(store.isLoading()).toBe(true);

    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY]);

    await expect(loaded).resolves.toBe(true);
    expect(store.workspace.projects()).toEqual({ organizationId: 'o1', status: 'loaded', items: [SUMMARY] });
    expect(store.isLoading()).toBe(false);
  });

  it('drops the answer for an organisation the header switched away from (R-SHL-03, TEN-01)', async () => {
    const first = store.loadProjects('o1');
    const second = store.loadProjects('o2');
    backend.expectOne('/api/v1/organizations/o2/projects').flush([]);
    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY]);

    await expect(second).resolves.toBe(true);
    await expect(first).resolves.toBe(false);
    expect(store.workspace.projects()).toEqual({ organizationId: 'o2', status: 'loaded', items: [] });
  });

  it('records a failed load as failed without rejecting', async () => {
    const loaded = store.loadProjects('o1');
    backend.expectOne('/api/v1/organizations/o1/projects').flush(null, { status: 500, statusText: 'Server Error' });

    await expect(loaded).resolves.toBe(true);
    expect(store.workspace.projects().status).toBe('failed');
    expect(store.isLoading()).toBe(false);
  });

  it('adds a created project to the loaded list of its organisation (R-PRJ-02)', async () => {
    const loaded = store.loadProjects('o1');
    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY]);
    await loaded;

    const created = store.createProject('o1', 'Neu');
    backend.expectOne('/api/v1/organizations/o1/projects').flush(CREATED, { status: 201, statusText: 'Created', headers: { ETag: CREATED.etag } });

    await expect(created).resolves.toEqual(CREATED);
    expect(store.workspace.projects().items).toEqual([
      { id: 'p2', name: 'Neu', status: 'open', productCount: 0, modifiedAt: '2026-11-18T09:00:00Z' },
      SUMMARY,
    ]);
  });

  it('leaves the list of another organisation alone when a project is created', async () => {
    const loaded = store.loadProjects('o2');
    backend.expectOne('/api/v1/organizations/o2/projects').flush([]);
    await loaded;

    const created = store.createProject('o1', 'Neu');
    backend.expectOne('/api/v1/organizations/o1/projects').flush(CREATED, { status: 201, statusText: 'Created', headers: { ETag: CREATED.etag } });
    await created;

    expect(store.workspace.projects().items).toEqual([]);
  });

  it('changes a project and shows the change in the loaded list of its organisation (R-PRJ-05)', async () => {
    const loaded = store.loadProjects('o1');
    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY, { ...SUMMARY, id: 'p3', name: 'Andere' }]);
    await loaded;

    const changed = store.changeProject('p1', { name: 'Umbenannt', status: 'done' }, '"1-3"');
    const request = backend.expectOne('/api/v1/projects/p1');
    expect(request.request.body).toEqual({ name: 'Umbenannt', status: 'done' });
    expect(request.request.headers.get('If-Match')).toBe('"1-3"');
    const answer: Project = { ...CREATED, id: 'p1', etag: '"1-4"', name: 'Umbenannt', status: 'done', productCount: 2, modifiedAt: '2026-11-18T10:00:00Z' };
    const { etag, ...body } = answer;
    request.flush(body, { headers: { ETag: etag } });

    await expect(changed).resolves.toEqual(answer);
    expect(store.workspace.projects().items).toEqual([
      { id: 'p1', name: 'Umbenannt', status: 'done', productCount: 2, modifiedAt: '2026-11-18T10:00:00Z' },
      { ...SUMMARY, id: 'p3', name: 'Andere' },
    ]);
  });

  it('rejects with the error of a refused change and keeps the list', async () => {
    const loaded = store.loadProjects('o1');
    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY]);
    await loaded;

    const changed = store.changeProject('p1', { name: 'Umbenannt' }, '"1-3"');
    backend.expectOne('/api/v1/projects/p1').flush(null, { status: 409, statusText: 'Conflict' });

    await expect(changed).rejects.toMatchObject({ status: 409 });
    expect(store.workspace.projects().items).toEqual([SUMMARY]);
  });

  it('rejects with the error of a refused creation', async () => {
    const created = store.createProject('o1', 'Neu');
    backend.expectOne('/api/v1/organizations/o1/projects').flush(null, { status: 403, statusText: 'Forbidden' });

    await expect(created).rejects.toMatchObject({ status: 403 });
  });

  it('forgets the projects on reset and drops a running load', async () => {
    const loaded = store.loadProjects('o1');
    store.resetWorkspace();
    backend.expectOne('/api/v1/organizations/o1/projects').flush([SUMMARY]);

    await expect(loaded).resolves.toBe(false);
    expect(store.workspace.projects()).toEqual({ organizationId: null, status: 'idle', items: [] });
  });

  it('assigns a list to a project and resolves to its copy (R-PRJ-02, R-PRJ-04)', async () => {
    const assigned = store.assignCriteriaList('p2', 'list-1', '"1-1"');
    const request = backend.expectOne('/api/v1/projects/p2/criteria-configuration');
    expect(request.request.body).toEqual({ sourceConfigurationId: 'list-1' });
    expect(request.request.headers.get('If-Match')).toBe('"1-1"');
    request.flush({ id: 'copy-1', name: 'HR', releaseVersion: 'T.1' });

    await expect(assigned).resolves.toEqual({ id: 'copy-1', name: 'HR', releaseVersion: 'T.1' });
  });

  it('keeps the criteria S11 of a copy took in for the next S32 of that project only, once (R-PRJ-06)', () => {
    store.noteCriteriaAdded('p1', 3);
    expect(store.takeCriteriaAdded('p2')).toBe(0);
    expect(store.takeCriteriaAdded('p1')).toBe(0);

    store.noteCriteriaAdded('p1', 3);
    expect(store.takeCriteriaAdded('p1')).toBe(3);
    expect(store.takeCriteriaAdded('p1')).toBe(0);

    store.noteCriteriaAdded('p1', 2);
    store.noteCriteriaAdded('p1', 0);
    expect(store.takeCriteriaAdded('p1')).toBe(0);
  });

  it('forgets a waiting report of added criteria on reset', () => {
    store.noteCriteriaAdded('p1', 1);
    store.resetWorkspace();
    expect(store.takeCriteriaAdded('p1')).toBe(0);
  });
});
