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
  filterTypeId: number;

  constructor(pId: number, pName: string, pDescription: string, pFilterTypeId: number) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.filterTypeId = pFilterTypeId;
  }
}
