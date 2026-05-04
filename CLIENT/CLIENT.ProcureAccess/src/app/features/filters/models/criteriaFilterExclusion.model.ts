import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class CriteriaFilterExclusion {
  @jsonMember
  id: number;

  @jsonMember
  criteriaFilterId: number;

  @jsonMember
  exclusionId: number;

  constructor(pId: number, pCriteriaFilterId: number, pExclusionId: number) {
    this.id = pId;
    this.criteriaFilterId = pCriteriaFilterId;
    this.exclusionId = pExclusionId;
  }
}
