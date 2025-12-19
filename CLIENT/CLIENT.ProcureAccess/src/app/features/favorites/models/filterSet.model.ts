import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { FilterTypeValue } from '@app/features/filters/models/filterTypeValue.model';

@jsonObject
export class FilterSet {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  userId: string;

  @jsonMember
  filterTypeValues: FilterTypeValue[];

  constructor(pId: number, pName: string, pUserId: string, pFilterTypeValues: FilterTypeValue[]) {
    this.id = pId;
    this.name = pName;
    this.userId = pUserId;
    this.filterTypeValues = pFilterTypeValues;
  }
}
