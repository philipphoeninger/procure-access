import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class Criterion {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  description: string;

  constructor(pId: number, pName: string, pDescription: string) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
  }
}
