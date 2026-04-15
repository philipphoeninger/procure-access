import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom } from "rxjs";
import { ProposalDto } from "../../models/proposal.dto";
import { UpsertProposal } from "../../models/upsert-proposal-request.model";
import { TResult } from "@app/core/models/result.model";
import { ReviewProposal } from "../../models/review-proposal-request.model";
import { ApproveProposal } from "../../models/approve-proposal-request.model";

@Injectable({ providedIn:'root' })
export class ProposalApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient) {}

  getProposals(): Promise<TResult<ProposalDto[]>> {
    return lastValueFrom(
        this.http.get<TResult<ProposalDto[]>>(`${this.apiUrl}/Proposals`)
    );
  }

  getProposal(proposalId: number): Promise<TResult<ProposalDto>> {
    return lastValueFrom(
        this.http.get<TResult<ProposalDto>>(`${this.apiUrl}/Proposals/${proposalId}`)
    );
  }

  upsertProposal(request: UpsertProposal): Promise<ProposalDto> {
    return lastValueFrom(
        this.http.post<ProposalDto>(`${this.apiUrl}/Proposals/upsert`, request)
    );
  }

  reviewProposal(request: ReviewProposal): Promise<ProposalDto> {
    return lastValueFrom(
        this.http.post<ProposalDto>(`${this.apiUrl}/Proposals/review`, request)
    );
  }

  approveProposal(request: ApproveProposal): Promise<ProposalDto> {
    return lastValueFrom(
        this.http.post<ProposalDto>(`${this.apiUrl}/Proposals/approve`, request)
    );
  }
}
