// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpContext } from '@angular/common/http';
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
          try {
              const result = await proposalApiService.getProposals();
              this.setProposals(result.value ?? []);
          } finally {
              state.decrementLoadingCount();
          }
      },
      setProposals(proposals: ProposalDto[]) {
          patchState(state, {
              proposals
          });
      },
      getProposalById(proposalId: number) {
          return state.proposals().find(proposal => proposal.id === proposalId) ?? null;
      },
      // `context` marks the request, e.g. FORM_SHOWS_FIELD_ERRORS; a failure is re-thrown to the form.
      async upsertProposal(command: UpsertProposal, context?: HttpContext) {
          state.incrementLoadingCount();
          try {
              await proposalApiService.upsertProposal(command, context);
              await this.loadProposals();
          } finally {
              state.decrementLoadingCount();
          }
      },
      async reviewProposal(command: ReviewProposal, context?: HttpContext) {
          state.incrementLoadingCount();
          try {
              await proposalApiService.reviewProposal(command, context);
              await this.loadProposals();
          } finally {
              state.decrementLoadingCount();
          }
      },
      async approveProposal(command: ApproveProposal) {
          state.incrementLoadingCount();
          try {
              await proposalApiService.approveProposal(command);
              await this.loadProposals();
          } finally {
              state.decrementLoadingCount();
          }
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
