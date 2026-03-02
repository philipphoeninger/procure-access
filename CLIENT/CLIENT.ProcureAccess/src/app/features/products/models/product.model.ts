import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';
import { ProductPart } from './productPart.model';
import { ProductTest } from './productTest.model';

@jsonObject
export class Product {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  link: string;

  @jsonMember
  description: string;

  @jsonMember
  typeId: number;

  @jsonArrayMember(ProductPart)
  parts: ProductPart[];

  @jsonArrayMember(ProductTest)
  tests: ProductTest[];

  constructor(
    pId: number, 
    pName: string, 
    pType: number, 
    pLink: string = "", 
    pDescription: string = "", 
    pParts: ProductPart[] = [],
    pTests: ProductTest[] = []) {
      this.id = pId;
      this.name = pName;
      this.typeId = pType;
      this.link = pLink;
      this.description = pDescription;
      this.parts = pParts;
      this.tests = pTests;
  }
}