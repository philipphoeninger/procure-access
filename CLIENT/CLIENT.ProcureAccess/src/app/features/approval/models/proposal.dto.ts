import { CriterionDto } from '@app/features/criteria/models/criterion.dto';
import { ProductDto } from '@app/features/products/models/product.dto';
import { ProposalStatus } from '@app/features/products/models/proposal-status.enum';
import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ProposalDto {
  @jsonMember
  id: number;

  @jsonMember
  proposerId: string;

  @jsonMember
  approverId?: string;

  @jsonMember
  product?: ProductDto;

  @jsonMember
  criterion?: CriterionDto;

  @jsonMember
  status: ProposalStatus;

  @jsonMember
  note?: string;

  @jsonMember
  createdAt: Date;

  @jsonMember
  finishedAt?: Date;

  constructor(
    pId: number, 
    pProposerId: string, 
    pApproverId: string | undefined,
    pProduct: ProductDto | undefined, 
    pCriterion: CriterionDto | undefined, 
    pStatus: ProposalStatus,
    pNote: string | undefined, 
    pCreatedAt: Date,
    pFinishedAt: Date | undefined) {
    this.id = pId;
    this.proposerId = pProposerId;
    this.approverId = pApproverId;
    this.product = pProduct;
    this.criterion = pCriterion;
    this.status = pStatus;
    this.note = pNote;
    this.createdAt = pCreatedAt;
    this.finishedAt = pFinishedAt;
  }
}
