import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ApproveProposal {
  @jsonMember
  proposalId: number;

  @jsonMember
  isApproved: boolean;

  @jsonMember
  note?: string;

  constructor(
    pProposalId: number,
    pIsApproved: boolean,
    pNote: string | undefined) {
    this.proposalId = pProposalId;
    this.isApproved = pIsApproved;
    this.note = pNote;
  }
}
