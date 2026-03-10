import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';
import { CriteriaFilter } from './criteriaFilter.model';

@jsonObject
export class FilterType {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  description: string;

  @jsonArrayMember(CriteriaFilter)
  criteriaFilters: CriteriaFilter[];

  constructor(pId: number, pName: string, pDescription: string = '', pCriteriaFilters: CriteriaFilter[] = []) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.criteriaFilters = pCriteriaFilters;
  }
}
