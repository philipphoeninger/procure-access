import { CreateCriterionDto } from '@app/features/criteria/models/create-criterion.dto';
import { CreateProductDto } from '@app/features/products/models/create-product.dto';
import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ReviewProposal {
  @jsonMember
  proposalId: number;

  @jsonMember
  product?: CreateProductDto;

  @jsonMember
  criterion?: CreateCriterionDto;

  constructor(
    pId: number,
    pProduct: CreateProductDto | undefined,
    pCriterion: CreateCriterionDto | undefined) {
    this.proposalId = pId;
    this.product = pProduct;
    this.criterion = pCriterion;
  }
}
