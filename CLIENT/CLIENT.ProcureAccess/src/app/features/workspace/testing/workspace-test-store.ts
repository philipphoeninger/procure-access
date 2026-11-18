// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { signalStore, withMethods } from '@ngrx/signals';

import { OrganizationSummary } from '@app/features/organization/models/organization';
import { withLoading } from '@app/shared/state/with-loading';

import { withWorkspace } from '../state/with-workspace';

/** The organisation the header shows in a test of S30 or S31; null without a membership. */
export const testActiveOrganization = signal<OrganizationSummary | null>(null);

/**
 * The root store as S30 and S31 read it: the real workspace slice (with the loading counter it
 * needs, S6-24) and the active organisation of {@link testActiveOrganization}. Provide
 * `ProjectsApiService` with a double; provide it as `{ provide: ProcureAccessStore, useClass: WorkspaceTestStore }`.
 */
export const WorkspaceTestStore = signalStore(
  withLoading(),
  withWorkspace(),
  withMethods(() => ({ activeOrganization: () => testActiveOrganization() })),
);
