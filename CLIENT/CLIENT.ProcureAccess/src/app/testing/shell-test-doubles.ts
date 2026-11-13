// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, Provider, signal } from '@angular/core';
import { Routes } from '@angular/router';
import { of } from 'rxjs';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@features/identity/services/auth.service';

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
  'product-proposal/:id',
  'profile',
  'settings',
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

export function createShellDoubles(signedIn = false) {
  const store = {
    user: signal<object | null>(signedIn ? { id: 'u1' } : null),
    isLoading: signal(false),
    signedIn: signal(signedIn),
    signOut: vi.fn().mockResolvedValue(undefined),
    setProposals: vi.fn(),
    reloadUICustomization: vi.fn().mockResolvedValue(undefined),
  };
  const authService = { showLogin: vi.fn() };
  const providers: Provider[] = [
    { provide: ProcureAccessStore, useValue: store },
    { provide: AuthService, useValue: authService },
  ];
  return { store, authService, providers };
}
