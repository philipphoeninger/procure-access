import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { CriteriaFilter } from '@app/features/filters/models/criteriaFilter.model';

@jsonObject
export class FilterSet {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  userId: string;

  @jsonMember
  criteriaFilter: CriteriaFilter[];

  constructor(pId: number, pName: string, pUserId: string, pCriteriaFilter: CriteriaFilter[]) {
    this.id = pId;
    this.name = pName;
    this.userId = pUserId;
    this.criteriaFilter = pCriteriaFilter;
  }
}
