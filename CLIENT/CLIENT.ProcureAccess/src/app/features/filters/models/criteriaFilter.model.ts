// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import {jsonObject, jsonMember, TypedJSON, jsonArrayMember} from 'typedjson';
import { FilterType } from './filterType.model';
import { Criterion } from '@app/features/criteria/models/criterion.model';
import { Lookup } from '@core/models/lookup.model';

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

  @jsonArrayMember(Lookup)
  exclusions: Lookup[];

  constructor(
    pId: number,
    pName: string,
    pDescription: string,
    pFilterTypeId: number,
    pExclusions: Lookup[]) {
    this.id = pId;
    this.name = pName;
    this.description = pDescription;
    this.filterTypeId = pFilterTypeId;
    this.exclusions = pExclusions;
  }
}
