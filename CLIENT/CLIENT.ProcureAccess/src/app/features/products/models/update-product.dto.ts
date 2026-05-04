import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';
import { ProductPart } from './productPart.model';
import { ProductTest } from './productTest.model';

@jsonObject
export class UpdateProductDto {
  @jsonMember
  name?: string;

  @jsonMember
  link?: string;

  @jsonMember
  description?: string;

  // TODO
  // @jsonMember
  // typeId: number;

  // @jsonArrayMember(ProductPart)
  // parts: ProductPart[];

  // @jsonArrayMember(ProductTest)
  // tests: ProductTest[];

  constructor(
    pName: string | undefined, 
    // pType: number, 
    pLink: string | undefined, 
    pDescription: string | undefined, 
    // pParts: ProductPart[] = [],
    // pTests: ProductTest[] = []
  ) {
      this.name = pName;
      // this.typeId = pType;
      this.link = pLink;
      this.description = pDescription;
      // this.parts = pParts;
      // this.tests = pTests;
  }
}