// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable, signal } from '@angular/core';

import { WeightRange } from '../models/criteria-list';
import { ConfiguratorApiService } from '../services/api/configurator-api.service';

/**
 * The settings of the configurator the server decides (SPEC §8.11; S5-22, TD-45): the 🔧 range of
 * a user weight (R-LST-03, SPEC SD23), from `GET /api/v1/configurator/settings`, so the client keeps
 * no copy of `Configurator:PriorityWeight`. Loaded once per page load; a failure is not kept, so
 * the next {@link ready} asks again.
 *
 * Every page that shows a criterion card waits for {@link ready} before it opens: S11 through its
 * guard, a saved list through its resolver. {@link weightRange} is read only there.
 */
@Injectable({ providedIn: 'root' })
export class ConfiguratorSettings {
  private readonly api = inject(ConfiguratorApiService);
  private readonly range = signal<WeightRange | null>(null);
  private loading: Promise<boolean> | null = null;

  /**
   * True once the settings are here. A failure resolves to false: the request is not marked, so
   * the error interceptor has shown the system state already (offline notice or server page).
   */
  ready(): Promise<boolean> {
    if (this.range() !== null) return Promise.resolve(true);
    this.loading ??= this.api.settings().then(
      (settings) => {
        this.range.set(settings.priorityWeight);
        return true;
      },
      () => {
        this.loading = null;
        return false;
      },
    );
    return this.loading;
  }

  /** The range of a user weight; only after {@link ready} resolved to true. */
  weightRange(): WeightRange {
    const range = this.range();
    if (range === null) {
      throw new Error('ConfiguratorSettings.weightRange() read before ready() resolved');
    }
    return range;
  }
}
