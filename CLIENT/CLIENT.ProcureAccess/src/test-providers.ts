// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { EnvironmentProviders, Provider, provideZonelessChangeDetection } from '@angular/core';

// Providers added to every TestBed (angular.json → test → providersFile).
// Tests run with the same zoneless change detection as production (TESTING §3.2).
const testProviders: (Provider | EnvironmentProviders)[] = [provideZonelessChangeDetection()];

export default testProviders;
