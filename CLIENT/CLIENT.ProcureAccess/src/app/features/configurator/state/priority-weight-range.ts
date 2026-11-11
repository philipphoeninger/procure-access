// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { InjectionToken } from '@angular/core';

import { environment } from '../../../../environments/environment';
import { WeightRange } from '../models/criteria-list';

/**
 * The range a user weight of a B criterion may take (R-LST-03 „editable 1–10“, a 🔧 value confirmed
 * by SPEC SD23). Read from the environment file of the build, like the criteria-suggestion channel
 * (C110), so it is never written into code; the server checks the same range with the export.
 */
export const PRIORITY_WEIGHT_RANGE = new InjectionToken<WeightRange>('PRIORITY_WEIGHT_RANGE', {
  providedIn: 'root',
  factory: () => environment.priorityWeight,
});
