// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';

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
  organizationId: 'org-1',
  name: 'Vergabe Personalsoftware 2027',
  releaseVersion: 'T.1',
  createdAt: '2026-11-16T09:00:00Z',
  modifiedAt: '2026-11-16T09:00:00Z',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: [] },
  criteria: [],
};

describe('savedListResolver (SPEC §8.10, §8.11)', () => {
  let settingsReady: boolean;
  let load: SavedListLoad;

  beforeEach(() => {
    settingsReady = true;
    load = new SavedListLoad();
    TestBed.configureTestingModule({
      providers: [
        { provide: SavedListLoad, useValue: load },
        { provide: ProcureAccessStore, useValue: { incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() } },
        { provide: ConfiguratorApiService, useValue: { get: () => Promise.resolve(LIST) } },
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
    ) as Promise<string>;

  it('shows the list once list and settings are loaded', async () => {
    expect(await resolve()).toBe('pageTitle.criteria.list.saved');
    expect(load.view.kind).toBe('list');
  });

  it('shows the server text in place when the settings fail (the weight range, TD-45)', async () => {
    settingsReady = false;

    expect(await resolve()).toBe('pageTitle.server');
    expect(load.view.kind).toBe('server');
  });
});
