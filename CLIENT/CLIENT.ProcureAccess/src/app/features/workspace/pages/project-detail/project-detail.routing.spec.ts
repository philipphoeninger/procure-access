// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { Criterion, PriorityLevel } from '@app/features/configurator/models/criteria';
import { SavedList } from '@app/features/configurator/models/saved-lists';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { ProjectLoad } from './project-detail-load';
import { projectResolver } from './project-detail.routing';

const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  etag: '"1-1"',
  name: 'Bewerbermanagement',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
  criteriaListLocked: false,
  candidates: [],
};

const WITH_LIST: Project = { ...PROJECT, criteriaList: { id: 'copy-1', name: 'HR-Software 2027', releaseVersion: 'T.1' } };

/**
 * The project's copy: criterion 2 excluded, criterion 5 raised from B to A, the procedure criterion
 * included; criterion 4 has no stored row and keeps its recommendation.
 */
const COPY = {
  id: 'copy-1',
  name: 'HR-Software 2027',
  releaseVersion: 'T.1',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web', 'app'], productPartIds: ['base'], testTypeIds: ['softwareTest', 'bikWeb'] },
  criteria: [
    { criterionRevisionId: 1, isIncluded: true, overrideLevel: null, overrideWeight: null, note: null },
    { criterionRevisionId: 2, isIncluded: false, overrideLevel: null, overrideWeight: null, note: null },
    { criterionRevisionId: 3, isIncluded: true, overrideLevel: null, overrideWeight: null, note: null },
    { criterionRevisionId: 5, isIncluded: true, overrideLevel: 'A', overrideWeight: null, note: null },
  ],
} as unknown as SavedList;

const criterion = (revisionId: number, key: string, kind: Criterion['kind'], level: PriorityLevel) =>
  ({ revisionId, key, kind, resolvedPriority: { level } }) as Criterion;

const CRITERIA = {
  release: 'T.1',
  criteria: [
    criterion(1, '11.1.1.1', 'requirement', 'A'),
    criterion(2, '11.1.1.2', 'requirement', 'B'),
    criterion(3, 'PROC-softwareTest', 'testProcedure', 'Info'),
    criterion(4, '11.1.1.3', 'requirement', 'Info'),
    criterion(5, '11.1.1.4', 'requirement', 'B'),
  ],
  appTypesWithoutCriteria: [],
};

const OPTIONS = {
  release: 'T.1',
  layers: {
    useCases: [{ id: 'hr', name: 'HR-Anwendung' }],
    appTypes: [
      { id: 'app', name: 'Mobile App', excludedBy: [], hint: null },
      { id: 'web', name: 'Website oder Webanwendung', excludedBy: [], hint: null },
    ],
    productParts: [],
    testTypes: [
      { id: 'softwareTest', name: 'BIT inklusiv Software-Test' },
      { id: 'bikWeb', name: 'BIK BITV-Test (Web)' },
    ],
  },
};

describe('projectResolver (S32)', () => {
  let load: ProjectLoad;
  let get: ReturnType<typeof vi.fn>;
  let getList: ReturnType<typeof vi.fn>;

  function setUp(api: ReturnType<typeof vi.fn>, list: ReturnType<typeof vi.fn> = vi.fn().mockResolvedValue(COPY)) {
    load = new ProjectLoad();
    get = api;
    getList = list;
    TestBed.configureTestingModule({
      providers: [
        { provide: ProjectLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() } },
        { provide: ProjectsApiService, useValue: { get } },
        { provide: ConfiguratorApiService, useValue: { get: getList } },
        {
          provide: CatalogApiService,
          useValue: { getCriteria: vi.fn().mockResolvedValue(CRITERIA), getFilterOptions: vi.fn().mockResolvedValue(OPTIONS) },
        },
      ],
    });
  }

  const resolve = () =>
    TestBed.runInInjectionContext(() =>
      projectResolver({ paramMap: convertToParamMap({ id: 'p-1' }) } as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as Promise<string>;

  it('loads the project and names the page title of S32; without a list nothing more is read', async () => {
    setUp(vi.fn().mockResolvedValue(PROJECT));

    expect(await resolve()).toBe('pageTitle.project');
    expect(get).toHaveBeenCalledWith('p-1');
    expect(getList).not.toHaveBeenCalled();
    expect(load.view).toEqual({ kind: 'project', project: PROJECT, criteria: null });
  });

  it('reads the copy: the included requirement criteria by the level that applies, „Geprüft nach …“ apart, the selection by name', async () => {
    setUp(vi.fn().mockResolvedValue(WITH_LIST));

    await resolve();

    expect(getList).toHaveBeenCalledWith('copy-1');
    expect(load.view).toEqual({
      kind: 'project',
      project: WITH_LIST,
      criteria: {
        listName: 'HR-Software 2027',
        releaseVersion: 'T.1',
        scope: { total: 3, a: 2, b: 0, info: 1, procedures: 1 },
        useCase: 'HR-Anwendung',
        appTypes: ['Mobile App', 'Website oder Webanwendung'],
        testTypes: ['BIT inklusiv Software-Test', 'BIK BITV-Test (Web)'],
      },
    });
  });

  it('shows the server text when the copy cannot be read', async () => {
    setUp(vi.fn().mockResolvedValue(WITH_LIST), vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 500 })));

    expect(await resolve()).toBe('pageTitle.server');
    expect(load.view.kind).toBe('server');
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
