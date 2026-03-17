import { computed, inject } from '@angular/core';
import { initialAppState } from '@app/core/models/appState.interface';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { Criterion } from '../models/criterion.model';
import { CriteriaApiService } from '../services/api/criteria-api.service';
import { withLoading } from '@app/shared/state/with-loading';
import { withFilters } from '@app/features/filters/state/with-filters';

export type CriteriaState = { criteria: Criterion[] };

export const withCriteria = () => signalStoreFeature(
    withState<CriteriaState>({ criteria: initialAppState.criteria }),
    withLoading(),
    withFilters(),
    withMethods((state, criteriaApiService = inject(CriteriaApiService)) => ({
      async getCriteriaBySelectedCriteriaFilterIds() {
          state.incrementLoadingCount();
          
          let criteria = await criteriaApiService.getCriteriaByCriteriaFilterIds(state.selectedCriteriaFilters());
          this.setCriteria(criteria);

          state.decrementLoadingCount();
      },
      setCriteria(criteria: Criterion[]) {
          patchState(state, {
              criteria
          });
      }
    })),
    withComputed((state) => ({
      criteriaCount: computed(() => {
        return state.criteria().length;
      })
    }))
  );
