// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';

@jsonObject
export class CreateProductDto {
  @jsonMember
  name: string;

  @jsonMember
  link?: string;

  @jsonMember
  description?: string;

  @jsonArrayMember(Number)
  criteriaFilterIds?: number[];

  constructor(
    pName: string,
    pLink: string | undefined,
    pDescription: string | undefined,
    pCriteriaFilterIds: number[] | undefined
  ) {
      this.name = pName;
      this.link = pLink;
      this.description = pDescription;
      this.criteriaFilterIds = pCriteriaFilterIds;
  }
}
