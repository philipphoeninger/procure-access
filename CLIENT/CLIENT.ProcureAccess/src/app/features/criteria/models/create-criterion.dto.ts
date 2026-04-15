import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class CreateCriterionDto {
  @jsonMember
  name: string;

  @jsonMember
  description: string;

  constructor(pName: string, pDescription: string) {
    this.name = pName;
    this.description = pDescription;
  }
}
