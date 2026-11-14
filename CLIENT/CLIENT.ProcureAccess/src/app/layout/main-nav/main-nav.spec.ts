// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
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
      ['Kriterien', '/kriterien'],
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

  it.each(['/kriterien', '/kriterien/ergebnis', '/filters'])(
    'marks „Kriterien“ as current on %s, also on the legacy filter that stands in for S10 (O-S4-3)',
    async (url) => {
      const { fixture } = await renderNav();

      await TestBed.inject(Router).navigateByUrl(url);
      await fixture.whenStable();

      expect(screen.getByRole('link', { name: 'Kriterien' }).getAttribute('aria-current')).toBe('page');
      expect(screen.getByRole('link', { name: 'Produkte' }).hasAttribute('aria-current')).toBe(false);
    },
  );

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

  // The session state is a signal (withIdentity(), S5-08): the nav follows it without a navigation,
  // also after an account deletion or a session that ended.
  it('shows „Anmelden“ again as soon as the session is gone', async () => {
    const { fixture, store } = await renderNav('bar', true);
    expect(screen.getByRole('button', { name: 'Abmelden' })).toBeTruthy();

    store.signedIn.set(false);
    await fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Abmelden' })).toBeNull();
  });

  it('ends the session, opens the start page and focuses its heading when „Abmelden“ is pressed (R-AUTH-08)', async () => {
    const { fixture, store } = await renderNav('bar', true);
    await TestBed.inject(Router).navigateByUrl('/products');
    const focusPageHeading = vi.spyOn(TestBed.inject(PageTitleStrategy), 'focusPageHeading').mockImplementation(() => undefined);

    screen.getByRole('button', { name: 'Abmelden' }).click();
    await fixture.whenStable();

    expect(store.endSession).toHaveBeenCalledOnce();
    expect(TestBed.inject(Router).url).toBe('/');
    expect(focusPageHeading).toHaveBeenCalledOnce();
  });

  it('opens the start page also when the sign-out request fails', async () => {
    const { fixture, store } = await renderNav('bar', true);
    store.endSession.mockRejectedValue(new Error('offline'));
    await TestBed.inject(Router).navigateByUrl('/products');

    screen.getByRole('button', { name: 'Abmelden' }).click();
    await fixture.whenStable();

    expect(TestBed.inject(Router).url).toBe('/');
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
