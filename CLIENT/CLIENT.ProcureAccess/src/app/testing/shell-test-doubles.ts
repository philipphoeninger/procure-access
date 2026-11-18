// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, computed, Provider, signal } from '@angular/core';
import { Routes } from '@angular/router';
import { of } from 'rxjs';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@features/identity/services/auth.service';
import { OrganizationSummary } from '@app/features/organization/models/organization';

// Test doubles for the shell components (header, main nav, footer, app shell): only what is
// outside the component is replaced — the layout breakpoint, the store (with the session state of
// withIdentity(), S5-08) and AuthService.

@Component({ template: '<h1>Seite</h1>' })
class TestPage {}

/** Every route the shell links to. */
export const SHELL_TEST_ROUTES: Routes = [
  '',
  'kriterien',
  'kriterien/ergebnis',
  'filters',
  'products',
  'konto',
  'kriterienlisten',
  'projekte',
  'projekte/:id',
  'einstellungen',
  'organisation',
  'organisation/:id/mitglieder',
  'impressum',
  'datenschutz',
  'barrierefreiheit',
].map((path) => ({ path, component: TestPage }));

export function provideLayout(wide: boolean): Provider {
  return {
    provide: BreakpointObserver,
    useValue: {
      observe: () => of({ matches: wide, breakpoints: {} }),
      isMatched: () => wide,
    },
  };
}

/** Two organisations of the signed-in test user, by name (`GET /api/v1/organizations`). */
export const TEST_ORGANIZATIONS: OrganizationSummary[] = [
  { id: 'org-a', name: 'Ämter Musterhausen', type: 'publicBody', role: 'owner' },
  { id: 'org-v', name: 'Verein Beispiel', type: 'association', role: 'reader' },
];

/**
 * The store as the shell reads it. `organizations` and the active one behave like `withIdentity()`
 * (S5-18): the choice of the switch while it is one of the list, otherwise the first by name.
 */
export function createShellDoubles(signedIn = false, organizations: OrganizationSummary[] = []) {
  const signedInState = signal(signedIn);
  const organizationsState = signal(organizations);
  const activeOrganizationId = signal<string | null>(null);
  const store = {
    user: signal<object | null>(signedIn ? { id: 'u1' } : null),
    isLoading: signal(false),
    signedIn: signedInState,
    session: computed(() => (signedInState() ? { id: 'u1', email: 'ada@example.test', roles: [], permissions: [] } : null)),
    organizations: organizationsState,
    activeOrganization: computed(
      () => organizationsState().find((o) => o.id === activeOrganizationId()) ?? organizationsState()[0] ?? null,
    ),
    switchOrganization: vi.fn((id: string) => activeOrganizationId.set(id)),
    endSession: vi.fn().mockResolvedValue(undefined),
    // No limits: the session warning (S5-30), which App starts, plans nothing.
    sessionExpiresAt: signal<string | null>(null),
    sessionAbsoluteExpiresAt: signal<string | null>(null),
    sessionWarningLeadSeconds: signal<number | null>(null),
  };
  const authService = { showLogin: vi.fn().mockResolvedValue(true) };
  const providers: Provider[] = [
    { provide: ProcureAccessStore, useValue: store },
    { provide: AuthService, useValue: authService },
  ];
  return { store, authService, providers };
}
