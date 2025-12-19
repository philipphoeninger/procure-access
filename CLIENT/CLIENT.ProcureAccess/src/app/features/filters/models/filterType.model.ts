import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';
import { FilterTypeValue } from './filterTypeValue.model';

@jsonObject
export class FilterType {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  displayQuestion: string;

  @jsonMember
  values: FilterTypeValue[];

  constructor(pId: number, pName: string, pValues: FilterTypeValue[], pDisplayQuestion: string) {
    this.id = pId;
    this.name = pName;
    this.values = pValues;
    this.displayQuestion = pDisplayQuestion;
  }
}
