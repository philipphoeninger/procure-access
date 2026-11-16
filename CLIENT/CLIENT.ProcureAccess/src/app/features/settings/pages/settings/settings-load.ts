// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable } from '@angular/core';

/** Whether the resolver of S60 could read the settings, handed to the page; one per route (route-level provider). */
@Injectable()
export class SettingsLoad {
  failed = false;
}
