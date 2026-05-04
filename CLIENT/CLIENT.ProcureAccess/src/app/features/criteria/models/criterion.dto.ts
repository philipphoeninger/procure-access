import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class CriterionDto {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  description: string;

  @jsonMember
  criteriaFilterId: number;

  constructor(
    pId: number, 
    pName: string, 
    pDescription: string, 
    pCriteriaFilterId: number) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.criteriaFilterId = pCriteriaFilterId;
  }
}
