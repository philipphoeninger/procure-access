// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { WeightRange } from './criteria-list';

/** The answer of `GET /api/v1/configurator/settings` (SPEC §8.11). */
export interface ConfiguratorSettingsResponse {
  priorityWeight: WeightRange;
}
