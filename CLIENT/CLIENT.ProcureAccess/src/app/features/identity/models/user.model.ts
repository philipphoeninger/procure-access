// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import 'reflect-metadata';
import { jsonObject, jsonMember } from 'typedjson';

@jsonObject
export class User {
  @jsonMember
  id: string;

  @jsonMember
  email: string;

  // @jsonMember
  // twoFAEnabled: boolean;

  constructor(pId: string, pEmail: string) {
    this.id = pId;
    this.email = pEmail;
    // this.twoFAEnabled = pTwoFAEnabled;
  }
}
