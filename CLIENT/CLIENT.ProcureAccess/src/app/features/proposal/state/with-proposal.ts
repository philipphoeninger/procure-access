import { computed, inject } from '@angular/core';
import { initialAppState } from '@app/core/models/appState.interface';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { withLoading } from '@app/shared/state/with-loading';
import { ProposalApiService } from '../services/api/proposal-api.service';
import { ProposalDto } from '../models/proposal.dto';
import { UpsertProposal } from '../models/upsert-proposal-request.model';
import { ReviewProposal } from '../models/review-proposal-request.model';
import { ApproveProposal } from '../models/approve-proposal-request.model';

export type ProposalState = { proposals: ProposalDto[] };

export const withProposal = () => signalStoreFeature(
    withState<ProposalState>({ proposals: initialAppState.proposals }),
    withLoading(),
    withMethods((state, proposalApiService = inject(ProposalApiService)) => ({
      async loadProposals() {
          state.incrementLoadingCount();
          
          let result = await proposalApiService.getProposals();
          this.setProposals(result.value ?? []);

          state.decrementLoadingCount();
      },
      setProposals(proposals: ProposalDto[]) {
          patchState(state, {
              proposals
          });
      },
      getProposalById(proposalId: number) {
          return state.proposals().find(proposal => proposal.id === proposalId) ?? null;
      },
      async upsertProposal(command: UpsertProposal) {
          state.incrementLoadingCount();
          await proposalApiService.upsertProposal(command);
          await this.loadProposals();
          state.decrementLoadingCount();
      },
      async reviewProposal(command: ReviewProposal) {
          state.incrementLoadingCount();
          await proposalApiService.reviewProposal(command);
          await this.loadProposals();
          state.decrementLoadingCount();
      },
      async approveProposal(command: ApproveProposal) {
          state.incrementLoadingCount();
          await proposalApiService.approveProposal(command);
          await this.loadProposals();
          state.decrementLoadingCount();
      }
    })),
    withComputed((state) => ({
      proposalsCount: computed(() => {
        return state.proposals().length;
      }),
      openProposals: computed(() => state.proposals().filter(x => !x.finishedAt)),
      closedProposals: computed(() => state.proposals().filter(x => x.finishedAt))
    })),
    withComputed((state) => ({
        openProposalsCount: computed(() => state.openProposals().length),
        closedProposalsCount: computed(() => state.closedProposals().length)
    }))
  );
