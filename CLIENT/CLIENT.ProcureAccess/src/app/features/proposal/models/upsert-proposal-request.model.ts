import { CreateCriterionDto } from '@app/features/criteria/models/create-criterion.dto';
import { CreateProductDto } from '@app/features/products/models/create-product.dto';
import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class UpsertProposal {
  @jsonMember
  id?: number;

  @jsonMember
  productId?: number;

  @jsonMember
  product?: CreateProductDto;

  @jsonMember
  criterionId?: number;

  @jsonMember
  criterion?: CreateCriterionDto;

  constructor(
    pId: number | undefined,
    pProductId: number | undefined,
    pProduct: CreateProductDto | undefined,
    pCriterionId: number | undefined,
    pCriterion: CreateCriterionDto | undefined) {
    this.id = pId;
    this.productId = pProductId;
    this.product = pProduct;
    this.criterionId = pCriterionId;
    this.criterion = pCriterion;
  }
}
