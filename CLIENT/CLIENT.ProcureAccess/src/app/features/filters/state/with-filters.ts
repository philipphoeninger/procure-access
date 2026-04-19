import { computed, inject } from '@angular/core';
import { initialAppState } from '@app/core/models/appState.interface';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { FilterType } from '../models/filterType.model';
import { withLoading } from '@app/shared/state/with-loading';
import { FiltersApiService } from '../services/api/filters-api.service';
import { CriteriaFilter } from '../models/criteriaFilter.model';
import { EnFilterTypeId } from '../models/filterTypes.enum';

export type FiltersState = { 
  filterTypes: FilterType[], 
  criteriaFilters: CriteriaFilter[], 
  selectedCriteriaFilters: number[] 
};

export const withFilters = () => signalStoreFeature(
    withState<FiltersState>({
      filterTypes: initialAppState.filters.filterTypes,
      criteriaFilters: initialAppState.filters.criteriaFilters,
      selectedCriteriaFilters: initialAppState.filters.selectedCriteriaFilters
    }),
    withLoading(),
    withMethods((state, filtersApiService = inject(FiltersApiService)) => ({
        async loadFilters() {
          state.incrementLoadingCount();
          // get Filter Types
          let filterTypesResult = await filtersApiService.getAllFilterTypes();
          // get & set Criteria Filters
          let criteriaFiltersResult = await filtersApiService.getAllCriteriaFilters();
          this.setCriteriaFilters(criteriaFiltersResult.value ?? []);
          // set Criteria Filters of each Filter Type & set Filter Types
          filterTypesResult.value?.forEach(filterType => {
            filterType.criteriaFilters = 
              state.criteriaFilters().filter(cf => cf.filterTypeId === filterType.id);
          });
          this.setFilterTypes(filterTypesResult.value ?? []);

          state.decrementLoadingCount();
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
        setSelectedCriteriaFilters(selectedCriteriaFilters: number[]) {
          patchState(state, {
            selectedCriteriaFilters: selectedCriteriaFilters
          });
        }
    })),
    withComputed((state) => ({
      selectedCriteriaFiltersCount: computed(() => {
        return state.selectedCriteriaFilters().length;
      }),
      productParts: computed(() => 
        state.criteriaFilters().filter(x => x.filterTypeId == EnFilterTypeId.productPart))
    }))
);
