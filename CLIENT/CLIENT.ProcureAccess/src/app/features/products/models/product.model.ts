import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class Product {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  link: string;

  @jsonMember
  functionality: string;

  @jsonMember
  type: string;

  constructor(pId: number, pName: string, pLink: string, pFunctionality: string, pType: string) {
    this.id = pId;
    this.name = pName;
    this.link = pLink;
    this.functionality = pFunctionality;
    this.type = pType;
  }
}