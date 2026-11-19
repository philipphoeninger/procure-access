// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { SavedList } from '@app/features/configurator/models/saved-lists';
import { SavedListLoad } from '@app/features/configurator/pages/saved-list/saved-list-load';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { ConfiguratorSettings } from '@app/features/configurator/state/configurator-settings';
import { criteriaAnswer } from '@app/features/configurator/testing/criteria-fixture';
import { filterOptionsAnswer } from '@app/features/configurator/testing/filter-options-fixture';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { projectCriteriaListResolver } from './project-criteria-list.routing';

const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  etag: '"1-1"',
  name: 'Bewerbermanagement',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: { id: 'copy-1', name: 'HR-Software 2027', releaseVersion: 'T.1' },
};

const COPY: SavedList = {
  id: 'copy-1',
  etag: '"1-1"',
  organizationId: 'org-1',
  name: 'HR-Software 2027',
  releaseVersion: 'T.1',
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  procurementProjectId: 'p-1',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] },
  criteria: [],
};

describe('projectCriteriaListResolver (S11 of a project copy, R-PRJ-06)', () => {
  let load: SavedListLoad;
  let getProject: ReturnType<typeof vi.fn>;
  let getList: ReturnType<typeof vi.fn>;
  let settingsReady: boolean;

  function setUp(project: ReturnType<typeof vi.fn>) {
    load = new SavedListLoad();
    getProject = project;
    getList = vi.fn().mockResolvedValue(COPY);
    settingsReady = true;
    TestBed.configureTestingModule({
      providers: [
        { provide: SavedListLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() } },
        { provide: ProjectsApiService, useValue: { get: getProject } },
        { provide: ConfiguratorApiService, useValue: { get: getList } },
        {
          provide: CatalogApiService,
          useValue: {
            getCriteria: () => Promise.resolve(criteriaAnswer()),
            getFilterOptions: () => Promise.resolve(filterOptionsAnswer()),
          },
        },
        { provide: ConfiguratorSettings, useValue: { ready: () => Promise.resolve(settingsReady) } },
      ],
    });
  }

  // The child route of `/projekte/:id`: the id is a parameter of a route above it.
  const resolve = () => {
    const parent = { paramMap: convertToParamMap({ id: 'p-1' }) } as ActivatedRouteSnapshot;
    const route = { paramMap: convertToParamMap({}), pathFromRoot: [parent] } as unknown as ActivatedRouteSnapshot;
    route.pathFromRoot.push(route);
    return TestBed.runInInjectionContext(() => projectCriteriaListResolver(route, {} as RouterStateSnapshot)) as Promise<string>;
  };

  it('loads the project, then its copy, and names the page title of the list of a project (D6 (a))', async () => {
    setUp(vi.fn().mockResolvedValue(PROJECT));

    expect(await resolve()).toBe('pageTitle.project.criteria');
    expect(getProject).toHaveBeenCalledWith('p-1');
    expect(getList).toHaveBeenCalledWith('copy-1');
    expect(load.view.kind === 'list' && load.view.project).toEqual({ id: 'p-1', name: 'Bewerbermanagement', archived: false });
  });

  it('marks the copy of an archived project (owner decision S6-12 D3 (a))', async () => {
    setUp(vi.fn().mockResolvedValue({ ...PROJECT, status: 'archived' }));

    await resolve();
    expect(load.view.kind === 'list' && load.view.project?.archived).toBe(true);
  });

  it('shows „Seite nicht gefunden“ for a project without a list', async () => {
    setUp(vi.fn().mockResolvedValue({ ...PROJECT, criteriaList: null }));

    expect(await resolve()).toBe('pageTitle.notFound');
    expect(getList).not.toHaveBeenCalled();
  });

  it('INV:TEN-01 shows „Seite nicht gefunden“ for a project the user cannot see (404)', async () => {
    setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 404 })));

    expect(await resolve()).toBe('pageTitle.notFound');
  });

  it('shows the server text for any other failure, also of the settings', async () => {
    setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 500 })));
    expect(await resolve()).toBe('pageTitle.server');

    TestBed.resetTestingModule();
    setUp(vi.fn().mockResolvedValue(PROJECT));
    settingsReady = false;
    expect(await resolve()).toBe('pageTitle.server');
  });
});
