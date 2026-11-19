// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, provideRouter, RedirectCommand, RouterStateSnapshot } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { SavedList } from '../../models/saved-lists';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ConfiguratorSettings } from '../../state/configurator-settings';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { SavedListLoad } from './saved-list-load';
import { savedListResolver } from './saved-list.routing';

const LIST: SavedList = {
  id: 'list-1',
  etag: '"1-1"',
  organizationId: 'org-1',
  name: 'Vergabe Personalsoftware 2027',
  releaseVersion: 'T.1',
  createdAt: '2026-11-16T09:00:00Z',
  modifiedAt: '2026-11-16T09:00:00Z',
  procurementProjectId: null,
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] },
  criteria: [],
};

describe('savedListResolver (SPEC §8.10, §8.11)', () => {
  let settingsReady: boolean;
  let load: SavedListLoad;
  let list: SavedList;

  beforeEach(() => {
    settingsReady = true;
    list = LIST;
    load = new SavedListLoad();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: SavedListLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() } },
        { provide: ConfiguratorApiService, useValue: { get: () => Promise.resolve(list) } },
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
  });

  const resolve = () =>
    TestBed.runInInjectionContext(() =>
      savedListResolver({ paramMap: convertToParamMap({ id: 'list-1' }) } as ActivatedRouteSnapshot, {} as RouterStateSnapshot),
    ) as Promise<string | RedirectCommand>;

  it('shows the list once list and settings are loaded', async () => {
    expect(await resolve()).toBe('pageTitle.criteria.list.saved');
    expect(load.view.kind).toBe('list');
  });

  it('reads the same list again for „Neue Fassung laden“ of S37 (S6-21b)', async () => {
    await resolve();
    list = { ...LIST, etag: '"1-5"', name: 'Von anderen umbenannt' };

    const again = await load.reload();

    expect(again.kind === 'list' && [again.list.name, again.list.etag]).toEqual(['Von anderen umbenannt', '"1-5"']);
  });

  it('shows the server text in place when the settings fail (the weight range, TD-45)', async () => {
    settingsReady = false;

    expect(await resolve()).toBe('pageTitle.server');
    expect(load.view.kind).toBe('server');
  });

  it('leads the address of the copy of a project to the address of the project, replacing it (owner decision S6-12 D7 (a))', async () => {
    list = { ...LIST, procurementProjectId: 'project-1' };

    const result = await resolve();

    expect(result).toBeInstanceOf(RedirectCommand);
    const redirect = result as RedirectCommand;
    expect(redirect.redirectTo.toString()).toBe('/projekte/project-1/kriterienliste');
    expect(redirect.navigationBehaviorOptions).toEqual({ replaceUrl: true });
  });
});
