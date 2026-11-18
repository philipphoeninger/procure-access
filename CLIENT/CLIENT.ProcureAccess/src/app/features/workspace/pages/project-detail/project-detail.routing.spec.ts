// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { ProjectLoad } from './project-detail-load';
import { projectResolver } from './project-detail.routing';

const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  name: 'Bewerbermanagement',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
};

describe('projectResolver (S32)', () => {
  let load: ProjectLoad;
  let get: ReturnType<typeof vi.fn>;

  function setUp(api: ReturnType<typeof vi.fn>) {
    load = new ProjectLoad();
    get = api;
    TestBed.configureTestingModule({
      providers: [
        { provide: ProjectLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() } },
        { provide: ProjectsApiService, useValue: { get } },
      ],
    });
  }

  const resolve = () =>
    TestBed.runInInjectionContext(() =>
      projectResolver({ paramMap: convertToParamMap({ id: 'p-1' }) } as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as Promise<string>;

  it('loads the project and names the page title of S32', async () => {
    setUp(vi.fn().mockResolvedValue(PROJECT));

    expect(await resolve()).toBe('pageTitle.project');
    expect(get).toHaveBeenCalledWith('p-1');
    expect(load.view).toEqual({ kind: 'project', project: PROJECT });
  });

  it('INV:TEN-01 shows „Seite nicht gefunden“ for a project the user cannot see (404)', async () => {
    setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 404 })));

    expect(await resolve()).toBe('pageTitle.notFound');
    expect(load.view.kind).toBe('notFound');
  });

  it('shows the server text for any other failure', async () => {
    setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 500 })));

    expect(await resolve()).toBe('pageTitle.server');
    expect(load.view.kind).toBe('server');
  });
});
