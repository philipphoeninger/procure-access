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
import { CriteriaFilterExclusion } from '../models/criteriaFilterExclusion.model';

export type FiltersState = { 
  filterTypes: FilterType[], 
  criteriaFilters: CriteriaFilter[],
  criteriaFilterExclusions: CriteriaFilterExclusion[],
  selectedCriteriaFilters: number[]
};

export const withFilters = () => signalStoreFeature(
    withState<FiltersState>({
      filterTypes: initialAppState.filters.filterTypes,
      criteriaFilters: initialAppState.filters.criteriaFilters,
      criteriaFilterExclusions: initialAppState.filters.criteriaFilterExclusions,
      selectedCriteriaFilters: initialAppState.filters.selectedCriteriaFilters
    }),
    withLoading(),
    withMethods((state, filtersApiService = inject(FiltersApiService)) => ({
        async loadFilters() {
          state.incrementLoadingCount();
          // get Filter Types, Criteria Filters & Criteria Filter Exclusions
          const [
            filterTypesResult,
            criteriaFiltersResult,
            criteriaFilterExclusionsResult] =
            await Promise.all([
              filtersApiService.getAllFilterTypes(),
              filtersApiService.getAllCriteriaFilters(),
              filtersApiService.getAllCriteriaFilterExclusions()
            ]);
          // set Criteria Filters
          this.setCriteriaFilters(criteriaFiltersResult.value ?? []);
          // set Criteria Filters of each Filter Type & set Filter Types
          filterTypesResult.value?.forEach(filterType => {
            filterType.criteriaFilters = 
              state.criteriaFilters().filter(cf => cf.filterTypeId === filterType.id);
          });
          this.setFilterTypes(filterTypesResult.value ?? []);
          // set Criteria Filter Exclusions
          this.setCriteriaFilterExclusions(criteriaFilterExclusionsResult.value ?? []);

          state.decrementLoadingCount();
        },
        getCriteriaFilterByName(name: string) {
          return state.criteriaFilters().find(x => x.name == name);
        },
        getCriteriaFilterExclusionsByFilterIds(criteriaFilterIds: number[]) {
          let result: number[][] = [];
          criteriaFilterIds.forEach(x => {
            let exclusions = state.criteriaFilterExclusions()
              .filter(y => y.criteriaFilterId == x)
              .map(x => x.exclusionId);
            result.push(exclusions);
          })
          return result;
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
        setCriteriaFilterExclusions(criteriaFilterExclusions: CriteriaFilterExclusion[]) {
          patchState(state, {
            criteriaFilterExclusions
          })
        },
        setSelectedCriteriaFilters(selectedCriteriaFilters: number[]) {
          patchState(state, {
            selectedCriteriaFilters
          });
        }
    })),
    withComputed((state) => ({
      selectedCriteriaFiltersCount: computed(() => {
        return state.selectedCriteriaFilters().length;
      }),
      productTypes: computed(() =>
        state.criteriaFilters().filter(x => x.filterTypeId == EnFilterTypeId.productType)),
      appTypes: computed(() =>
        state.criteriaFilters().filter(x => x.filterTypeId == EnFilterTypeId.appType)),
      productParts: computed(() => 
        state.criteriaFilters().filter(x => x.filterTypeId == EnFilterTypeId.productPart)),
      testTypes: computed(() => 
        state.criteriaFilters().filter(x => x.filterTypeId == EnFilterTypeId.testType))
    }))
);
