// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable } from '@angular/core';

import { Account } from '../../models/account';

/** The account the resolver of S61 read, handed to the page; null when the read failed. One per route (route-level provider). */
@Injectable()
export class AccountLoad {
  account: Account | null = null;
}
