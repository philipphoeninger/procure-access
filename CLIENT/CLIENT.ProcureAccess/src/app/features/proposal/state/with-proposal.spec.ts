// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { TResult } from '@app/core/models/result.model';
import { ApproveProposal } from '../models/approve-proposal-request.model';
import { ProposalApiService } from '../services/api/proposal-api.service';
import { withProposal } from './with-proposal';

// #168 and S2-11 P6: a failed request must not leave the loading bar on (pa-loading-bar, R-SHL-09).
const ProposalTestStore = signalStore(withProposal());

function setUp(apiService: Partial<ProposalApiService>) {
  TestBed.configureTestingModule({
    providers: [ProposalTestStore, { provide: ProposalApiService, useValue: apiService }],
  });
  return TestBed.inject(ProposalTestStore);
}

// The store does not look at the reason; HttpClient would reject with an HttpErrorResponse.
const failure = new Error('request failed');

describe('withProposal', () => {
  it('stops loading when the proposal list request fails', async () => {
    const store = setUp({ getProposals: () => Promise.reject(failure) });

    await expect(store.loadProposals()).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });

  it('stops loading when approving a proposal fails', async () => {
    const store = setUp({
      approveProposal: () => Promise.reject(failure),
      getProposals: () => Promise.resolve(new TResult(true, undefined, [])),
    });

    await expect(store.approveProposal({} as ApproveProposal)).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });
});
