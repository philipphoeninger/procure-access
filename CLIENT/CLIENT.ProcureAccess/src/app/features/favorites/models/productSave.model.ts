import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ProductSave {
  @jsonMember
  id: number;

  @jsonMember
  userId: string;

  @jsonMember
  productId: string; // TODO: number

  constructor(pId: number, pUserId: string, pProductId: string) {
    this.id = pId;
    this.userId = pUserId;
    this.productId = pProductId;
  }
}
