import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ProductTest {
  @jsonMember
  id: number;

  @jsonMember
  productId: number;

  @jsonMember
  criteriaFilterId: number;

  @jsonMember
  name: string;

  constructor(pId: number, pProductId: number, pCriteriaFilterId: number, pName: string) {
    this.id = pId;
    this.productId = pProductId;
    this.criteriaFilterId = pCriteriaFilterId;
    this.name = pName;
  }
}

// TODO: add ProductType