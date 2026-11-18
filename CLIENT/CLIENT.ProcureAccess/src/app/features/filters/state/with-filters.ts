// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { FilterType } from '../models/filterType.model';
import { LOADING_INPUT } from '@app/shared/state/with-loading';
import { FiltersApiService } from '../services/api/filters-api.service';
import { CriteriaFilter } from '../models/criteriaFilter.model';
import { EnFilterTypeId } from '../models/filterTypes.enum';

export type FiltersState = {
  filterTypes: FilterType[],
  criteriaFilters: CriteriaFilter[],
  selectedCriteriaFilters: CriteriaFilter[]
};

export const withFilters = () => signalStoreFeature(
    LOADING_INPUT,
    withState<FiltersState>({
      filterTypes: [],
      criteriaFilters: [],
      selectedCriteriaFilters: []
    }),
    withMethods((state, filtersApiService = inject(FiltersApiService)) => ({
        async loadFilters() {
          state.incrementLoadingCount();
          try {
            // get Filter Types & Criteria Filters
            const [
              filterTypesResult,
              criteriaFiltersResult] =
              await Promise.all([
                filtersApiService.getAllFilterTypes(),
                filtersApiService.getAllCriteriaFilters()
              ]);
            // set Criteria Filters
            this.setCriteriaFilters(criteriaFiltersResult.value ?? []);
            // set Criteria Filters of each Filter Type & set Filter Types
            filterTypesResult.value?.forEach(filterType => {
              filterType.criteriaFilters =
                state.criteriaFilters().filter(cf => cf.filterTypeId === filterType.id);
            });
            this.setFilterTypes(filterTypesResult.value ?? []);
          } finally {
            state.decrementLoadingCount();
          }
        },
        getCriteriaFilterByName(name: string) {
          return state.criteriaFilters().find(x => x.name == name);
        },
        setFilterTypes(filterTypes: FilterType[]) {
          patchState(state, {
            filterTypes
          })
        },
        setCriteriaFilters(criteriaFilters: CriteriaFilter[]) {
          patchState(state, {
            criteriaFilters
          })
        },
        setSelectedCriteriaFilters(selectedCriteriaFilters: CriteriaFilter[]) {
          patchState(state, {
            selectedCriteriaFilters
          });
        }
    })),
    withComputed((state) => {
      // `filterTypeId` is the plain number the API sends; the enum names the four ids.
      const ofType = (type: EnFilterTypeId) =>
        computed(() => state.criteriaFilters().filter(x => x.filterTypeId === Number(type)));
      return {
        selectedCriteriaFiltersCount: computed(() => {
          return state.selectedCriteriaFilters().length;
        }),
        selectedCriteriaFiltersIds: computed(() =>
          state.selectedCriteriaFilters().map(x => x.id)
        ),
        productTypes: ofType(EnFilterTypeId.productType),
        appTypes: ofType(EnFilterTypeId.appType),
        productParts: ofType(EnFilterTypeId.productPart),
        testTypes: ofType(EnFilterTypeId.testType)
      };
    })
);
