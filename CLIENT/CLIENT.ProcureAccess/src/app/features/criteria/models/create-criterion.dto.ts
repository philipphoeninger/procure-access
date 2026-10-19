// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class CreateCriterionDto {
  @jsonMember
  name: string;

  @jsonMember
  description: string;

  @jsonMember
  criteriaFilterId: number;

  constructor(
    pName: string, 
    pDescription: string,
    pCriteriaFilter: number) {
    this.name = pName;
    this.description = pDescription;
    this.criteriaFilterId = pCriteriaFilter;
  }
}
