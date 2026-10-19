// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON, jsonArrayMember } from 'typedjson';
import { Lookup } from '@core/models/lookup.model';

@jsonObject
export class ProductDto {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  @jsonMember
  link?: string;

  @jsonMember
  description?: string;

  @jsonArrayMember(Lookup)
  criteriaFilters: Lookup[];

  // @jsonArrayMember(ProductTest)
  // tests: ProductTest[];

  constructor(
    pId: number,
    pName: string,
    pLink: string | undefined,
    pDescription: string | undefined,
    pCriteriaFilters: Lookup[] = []
  ) {
      this.id = pId;
      this.name = pName;
      this.link = pLink;
      this.description = pDescription;
      this.criteriaFilters = pCriteriaFilters;
  }
}
