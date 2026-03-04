import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { FilterType } from './filterType.model';
import { Criterion } from '@app/features/criteria/models/criterion.model';

@jsonObject
export class CriteriaFilter {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  description: string;

  @jsonMember
  filterType: FilterType;

  @jsonMember
  criteria: Criterion[];

  constructor(pId: number, pName: string, pDescription: string, pFilterType: FilterType, pCriteria: Criterion[]) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.filterType = pFilterType;
    this.criteria = pCriteria;
  }
}
