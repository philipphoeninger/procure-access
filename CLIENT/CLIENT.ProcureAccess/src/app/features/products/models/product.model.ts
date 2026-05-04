import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';

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

  constructor(
    pId: number,
    pName: string,
    pType: number,
    pLink: string = "",
    pDescription: string = "") {
      this.id = pId;
      this.name = pName;
      this.typeId = pType;
      this.link = pLink;
      this.description = pDescription;
  }
}
