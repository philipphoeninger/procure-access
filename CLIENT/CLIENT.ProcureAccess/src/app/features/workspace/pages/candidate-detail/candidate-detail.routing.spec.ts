// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { Criterion } from '@app/features/configurator/models/criteria';
import { SavedList } from '@app/features/configurator/models/saved-lists';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { OrganizationRole } from '@app/features/organization/models/organization';

import { Candidate } from '../../models/candidates';
import { Project } from '../../models/projects';
import { CandidatesApiService } from '../../services/api/candidates-api.service';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { TEST_CANDIDATE, TEST_PROJECT } from '../../testing/candidate-fixtures';
import { candidateFormResolver, candidateResolver } from './candidate-detail.routing';
import { CandidateLoad } from './candidate-load';

/** The project's copy with two app types; the criteria answer and the filter values of its release follow. */
const COPY = {
  id: 'list-1',
  name: 'HR-Software 2027',
  releaseVersion: 'T.1',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web', 'app'], productPartIds: ['base'], testTypeIds: ['softwareTest'] },
  criteria: [
    { criterionRevisionId: 1, isIncluded: true },
    { criterionRevisionId: 2, isIncluded: false },
    { criterionRevisionId: 3, isIncluded: true },
  ],
} as unknown as SavedList;

const criterion = (revisionId: number, key: string, kind: Criterion['kind']) => ({ revisionId, key, kind }) as Criterion;

const CRITERIA = {
  release: 'T.1',
  criteria: [
    criterion(1, '11.1.1.1', 'requirement'),
    criterion(2, '11.1.1.2', 'requirement'),
    criterion(3, 'PROC-softwareTest', 'testProcedure'),
    criterion(4, '11.1.1.3', 'requirement'),
  ],
  appTypesWithoutCriteria: [],
};

const OPTIONS = {
  release: 'T.1',
  layers: {
    useCases: [],
    appTypes: [
      { id: 'app', name: 'Mobile App', excludedBy: [], hint: null },
      { id: 'pc', name: 'Desktop-Anwendung', excludedBy: [], hint: null },
      { id: 'web', name: 'Website oder Webanwendung', excludedBy: [], hint: null },
    ],
    productParts: [],
    testTypes: [],
  },
};

describe('S33 and S34 resolvers', () => {
  let load: CandidateLoad;
  let projects: { get: ReturnType<typeof vi.fn> };
  let candidates: { get: ReturnType<typeof vi.fn> };
  let configurator: { get: ReturnType<typeof vi.fn> };

  /** A double that answers the value, or fails with it when it is an HTTP error. */
  const answer = (value: unknown) =>
    value instanceof HttpErrorResponse ? vi.fn().mockRejectedValue(value) : vi.fn().mockResolvedValue(value);

  function setUp(
    options: {
      project?: Project | HttpErrorResponse;
      candidate?: Candidate | HttpErrorResponse;
      copy?: SavedList | HttpErrorResponse;
      role?: OrganizationRole;
    } = {},
  ) {
    load = new CandidateLoad();
    projects = { get: answer(options.project ?? TEST_PROJECT) };
    candidates = { get: answer(options.candidate ?? TEST_CANDIDATE) };
    configurator = { get: answer(options.copy ?? COPY) };
    TestBed.configureTestingModule({
      providers: [
        { provide: CandidateLoad, useValue: load },
        {
          provide: ProcureAccessStore,
          useValue: {
            incrementLoadingCount: vi.fn(),
            decrementLoadingCount: vi.fn(),
            organizations: () => [{ id: 'org-1', name: 'Personalamt', type: 'publicBody', role: options.role ?? 'editor' }],
          },
        },
        { provide: ProjectsApiService, useValue: projects },
        { provide: CandidatesApiService, useValue: candidates },
        { provide: ConfiguratorApiService, useValue: configurator },
        {
          provide: CatalogApiService,
          useValue: {
            getCriteria: vi.fn().mockResolvedValue(CRITERIA),
            getFilterOptions: vi.fn().mockResolvedValue(OPTIONS),
          },
        },
      ],
    });
  }

  function route(params: Record<string, string>): ActivatedRouteSnapshot {
    const paramMap = convertToParamMap(params);
    return { paramMap, pathFromRoot: [{ paramMap: convertToParamMap({ id: 'p-1' }) }, { paramMap }] } as unknown as ActivatedRouteSnapshot;
  }

  const resolveCandidate = () =>
    TestBed.runInInjectionContext(() => candidateResolver(route({ pid: 'c-1' }), {} as RouterStateSnapshot)) as Promise<string>;
  const resolveForm = (params: Record<string, string>) =>
    TestBed.runInInjectionContext(() => candidateFormResolver(route(params), {} as RouterStateSnapshot)) as Promise<string>;

  describe('candidateResolver (S34)', () => {
    it('loads project, candidate and the copy: its name, release and app types in catalog order', async () => {
      setUp();

      expect(await resolveCandidate()).toBe('pageTitle.candidate');
      expect(projects.get).toHaveBeenCalledWith('p-1');
      expect(candidates.get).toHaveBeenCalledWith('c-1');
      expect(configurator.get).toHaveBeenCalledWith('list-1');
      expect(load.view()).toEqual({
        kind: 'candidate',
        project: TEST_PROJECT,
        candidate: TEST_CANDIDATE,
        criteria: {
          listName: 'HR-Software 2027',
          releaseVersion: 'T.1',
          appTypeNames: ['Mobile App', 'Website oder Webanwendung'],
        },
      });
    });

    it('reads no copy for a project without one', async () => {
      setUp({ project: { ...TEST_PROJECT, criteriaList: null } });

      expect(await resolveCandidate()).toBe('pageTitle.candidate');
      expect(configurator.get).not.toHaveBeenCalled();
      expect(load.view()).toMatchObject({ kind: 'candidate', criteria: null });
    });

    it('INV:TEN-01 shows „Seite nicht gefunden“ for a candidate the user cannot see (404)', async () => {
      setUp({ candidate: new HttpErrorResponse({ status: 404 }) });

      expect(await resolveCandidate()).toBe('pageTitle.notFound');
      expect(load.view().kind).toBe('notFound');
    });

    it('INV:TEN-01 shows „Seite nicht gefunden“ for a candidate of another project than the address names', async () => {
      setUp({ candidate: { ...TEST_CANDIDATE, projectId: 'p-2' } });

      expect(await resolveCandidate()).toBe('pageTitle.notFound');
    });

    it('shows the server text for any other failure, also of the copy', async () => {
      setUp({ project: new HttpErrorResponse({ status: 500 }) });
      expect(await resolveCandidate()).toBe('pageTitle.server');

      TestBed.resetTestingModule();
      setUp({ copy: new HttpErrorResponse({ status: 500 }) });
      expect(await resolveCandidate()).toBe('pageTitle.server');
      expect(load.view().kind).toBe('server');
    });
  });

  describe('candidateFormResolver (S33)', () => {
    it('names „Produkt hinzufügen“ for a new product and reads no candidate and no copy (D1 (a))', async () => {
      setUp();

      expect(await resolveForm({})).toBe('pageTitle.candidate.new');
      expect(candidates.get).not.toHaveBeenCalled();
      expect(configurator.get).not.toHaveBeenCalled();
      expect(load.view()).toEqual({ kind: 'candidate', project: TEST_PROJECT, candidate: null, criteria: null });
    });

    it('names „Produkt bearbeiten“ for a product and reads it', async () => {
      setUp({ role: 'owner' });

      expect(await resolveForm({ pid: 'c-1' })).toBe('pageTitle.candidate.edit');
      expect(load.view()).toMatchObject({ kind: 'candidate', candidate: TEST_CANDIDATE });
    });

    it('gives a Reader „Kein Zugriff“ (R-CAN-02: Editor and Owner only)', async () => {
      setUp({ role: 'reader' });

      expect(await resolveForm({})).toBe('pageTitle.forbidden');
    });

    it('INV:TEN-01 shows „Seite nicht gefunden“ for a project the user cannot see', async () => {
      setUp({ project: new HttpErrorResponse({ status: 404 }) });

      expect(await resolveForm({})).toBe('pageTitle.notFound');
    });
  });
});
