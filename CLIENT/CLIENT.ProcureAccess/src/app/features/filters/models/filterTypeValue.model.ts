import { Criterion } from '@app/features/criteria/models/criterion.model';
import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class FilterTypeValue {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  description: string;

  @jsonMember
  criteria: Criterion[];

  constructor(pId: number, pName: string, pDescription: string, pCriteria: Criterion[]) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.criteria = pCriteria;
  }
}
