// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Provider } from '@angular/core';

import { WeightRange } from '../models/criteria-list';
import { ConfiguratorSettings } from '../state/configurator-settings';

/** The range `appsettings.json` sets (SPEC SD23), for tests that do not test the loading. */
export const TEST_WEIGHT_RANGE: WeightRange = { min: 1, max: 10 };

/** Settings already loaded with `range`, for tests of pages behind the guard or resolver. */
export function provideLoadedConfiguratorSettings(range: WeightRange = TEST_WEIGHT_RANGE): Provider {
  const loaded: Pick<ConfiguratorSettings, 'ready' | 'weightRange'> = {
    ready: () => Promise.resolve(true),
    weightRange: () => range,
  };
  return { provide: ConfiguratorSettings, useValue: loaded };
}
