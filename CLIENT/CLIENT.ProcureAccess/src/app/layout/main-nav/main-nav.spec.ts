// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { createShellDoubles, SHELL_TEST_ROUTES } from '@app/testing/shell-test-doubles';
import { MainNav } from './main-nav';

async function renderNav(layout: 'bar' | 'drawer' = 'bar', signedIn = false) {
  const doubles = createShellDoubles(signedIn);
  const navigated = vi.fn();
  const { fixture } = await render(MainNav, {
    inputs: { layout },
    on: { navigated },
    routes: SHELL_TEST_ROUTES,
    providers: [provideTestTranslations(), ...doubles.providers],
  });
  return { fixture, navigated, ...doubles };
}

const mainNav = () => screen.getByRole('navigation', { name: 'Hauptnavigation' });

describe('MainNav', () => {
  it('lists Kriterien, Produkte and Beitragen in the main navigation (state Anonymous)', async () => {
    await renderNav();

    const links = within(mainNav()).getAllByRole('link');
    expect(links.map((link) => [link.textContent?.trim(), link.getAttribute('href')])).toEqual([
      ['Kriterien', '/filters'],
      ['Produkte', '/products'],
      ['Beitragen', '/product-proposal/0'],
    ]);
    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    expect(screen.queryByRole('link', { name: 'Einstellungen' })).toBeNull();
  });

  it('marks only the item of the current page with aria-current="page"', async () => {
    const { fixture } = await renderNav();

    await TestBed.inject(Router).navigateByUrl('/products');
    await fixture.whenStable();

    expect(screen.getByRole('link', { name: 'Produkte' }).getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('link', { name: 'Kriterien' }).hasAttribute('aria-current')).toBe(false);
    expect(screen.getByRole('link', { name: 'Beitragen' }).hasAttribute('aria-current')).toBe(false);
  });

  it('opens the login and reports the action when „Anmelden“ is pressed', async () => {
    const { fixture, authService, navigated } = await renderNav();

    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();

    expect(authService.showLogin).toHaveBeenCalledOnce();
    expect(navigated).toHaveBeenCalled();
  });

  it('shows Konto, Einstellungen and Abmelden instead of Anmelden when signed in (O-S2-4)', async () => {
    await renderNav('bar', true);

    expect(screen.getByRole('link', { name: 'Konto' }).getAttribute('href')).toBe('/profile');
    expect(screen.getByRole('link', { name: 'Einstellungen' }).getAttribute('href')).toBe('/settings');
    expect(screen.getByRole('button', { name: 'Abmelden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Anmelden' })).toBeNull();
  });

  // Account deletion and a failed token refresh remove the token without resetting the user;
  // both navigate to /home.
  it('shows „Anmelden“ again after a navigation once the token is gone', async () => {
    let signedIn = true;
    const doubles = createShellDoubles(true);
    doubles.store.isAuthenticated = () => signedIn;
    const { fixture } = await render(MainNav, {
      inputs: { layout: 'bar' },
      routes: SHELL_TEST_ROUTES,
      providers: [provideTestTranslations(), ...doubles.providers],
    });
    expect(screen.getByRole('button', { name: 'Abmelden' })).toBeTruthy();

    signedIn = false;
    await TestBed.inject(Router).navigateByUrl('/impressum');
    await fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Abmelden' })).toBeNull();
  });

  it('signs out through the legacy services when „Abmelden“ is pressed', async () => {
    const { fixture, authService, store } = await renderNav('bar', true);

    screen.getByRole('button', { name: 'Abmelden' }).click();
    await fixture.whenStable();

    expect(authService.logout).toHaveBeenCalledOnce();
    expect(store.setUser).toHaveBeenCalledWith(null);
    expect(store.setProposals).toHaveBeenCalledWith([]);
    expect(store.reloadUICustomization).toHaveBeenCalledOnce();
  });

  it('marks the first item as the initial focus of the compact menu, and only there', async () => {
    await renderNav('drawer');
    expect(screen.getByRole('link', { name: 'Kriterien' }).hasAttribute('cdkFocusInitial')).toBe(true);
    expect(screen.getByRole('link', { name: 'Produkte' }).hasAttribute('cdkFocusInitial')).toBe(false);
  });

  it('sets no initial focus in the full header', async () => {
    await renderNav('bar');
    expect(screen.getByRole('link', { name: 'Kriterien' }).hasAttribute('cdkFocusInitial')).toBe(false);
  });
});
