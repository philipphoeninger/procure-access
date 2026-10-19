// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember, TypedJSON } from 'typedjson';

@jsonObject
export class ProductType {
  @jsonMember
  id: number;

  @jsonMember
  name: string;

  constructor(pId: number, pName: string) {
    this.id = pId;
    this.name = pName;
  }
}