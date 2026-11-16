// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { render, screen, within } from '@testing-library/angular';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { createShellDoubles, SHELL_TEST_ROUTES, TEST_ORGANIZATIONS } from '@app/testing/shell-test-doubles';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { MainNav } from './main-nav';

async function renderNav(layout: 'bar' | 'drawer' = 'bar', signedIn = false, organizations: OrganizationSummary[] = []) {
  const doubles = createShellDoubles(signedIn, organizations);
  const navigated = vi.fn();
  const { fixture } = await render(MainNav, {
    inputs: { layout },
    on: { navigated },
    routes: SHELL_TEST_ROUTES,
    providers: [
      provideTestTranslations(),
      { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ...doubles.providers,
    ],
  });
  return { fixture, navigated, ...doubles };
}

const ONE_ORGANIZATION = [TEST_ORGANIZATIONS[0]];

/** Opens the menu „Konto“ of the full header and returns it. */
async function openAccountMenu(fixture: { whenStable(): Promise<unknown> }): Promise<HTMLElement> {
  screen.getByRole('button', { name: 'Konto, angemeldet als ada@example.test' }).click();
  await fixture.whenStable();
  return screen.getByRole('menu');
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

  describe('state SignedIn (R-SHL-02; S5-18)', () => {
    it('shows the organisation display and the menu „Konto“ instead of „Anmelden“', async () => {
      await renderNav('bar', true, ONE_ORGANIZATION);

      expect(screen.getByText('Organisation: Ämter Musterhausen')).toBeTruthy();
      const account = screen.getByRole('button', { name: 'Konto, angemeldet als ada@example.test' });
      expect(account.textContent?.trim()).toBe('Konto');
      expect(account.getAttribute('aria-haspopup')).toBe('menu');
      expect(screen.queryByRole('button', { name: 'Anmelden' })).toBeNull();
      // With one membership the display is text, not a switch.
      expect(screen.queryByRole('button', { name: /^Organisation:/ })).toBeNull();
    });

    it('lists Konto, Organisation, Einstellungen and Abmelden in the menu „Konto“; „Organisation“ opens S21', async () => {
      const { fixture } = await renderNav('bar', true, ONE_ORGANIZATION);

      const menu = await openAccountMenu(fixture);

      const items = within(menu).getAllByRole('menuitem');
      expect(items.map((item) => item.textContent?.trim())).toEqual(['Konto', 'Organisation', 'Einstellungen', 'Abmelden']);
      expect(items[0].getAttribute('href')).toBe('/profile');
      expect(items[1].getAttribute('href')).toBe('/organisation/org-a/mitglieder');
      expect(items[2].getAttribute('href')).toBe('/settings');
    });

    it('marks „Organisation“ in the menu as the current page on S20 and S21', async () => {
      const { fixture } = await renderNav('bar', true, ONE_ORGANIZATION);
      await TestBed.inject(Router).navigateByUrl('/organisation/org-a/mitglieder');

      const menu = await openAccountMenu(fixture);

      expect(within(menu).getByRole('menuitem', { name: 'Organisation' }).getAttribute('aria-current')).toBe('page');
      expect(within(menu).getByRole('menuitem', { name: 'Konto' }).hasAttribute('aria-current')).toBe(false);
    });

    it('shows Konto, Organisation, Einstellungen and Abmelden as a flat list in the compact menu', async () => {
      await renderNav('drawer', true, ONE_ORGANIZATION);

      expect(screen.getByText('Organisation: Ämter Musterhausen')).toBeTruthy();
      expect(screen.getByRole('link', { name: 'Konto' }).getAttribute('href')).toBe('/profile');
      expect(screen.getByRole('link', { name: 'Organisation' }).getAttribute('href')).toBe('/organisation/org-a/mitglieder');
      expect(screen.getByRole('link', { name: 'Einstellungen' }).getAttribute('href')).toBe('/settings');
      expect(screen.getByRole('button', { name: 'Abmelden' })).toBeTruthy();
      expect(screen.queryByRole('menu')).toBeNull();
    });
  });

  describe('state SignedInNoOrg (R-SHL-02; S5-18)', () => {
    it('shows no organisation display, and „Organisation“ opens S20', async () => {
      const { fixture } = await renderNav('bar', true);

      expect(screen.queryByText(/^Organisation:/)).toBeNull();
      const menu = await openAccountMenu(fixture);
      expect(within(menu).getByRole('menuitem', { name: 'Organisation' }).getAttribute('href')).toBe('/organisation');
    });

    it('shows no organisation display in the compact menu either', async () => {
      await renderNav('drawer', true);

      expect(screen.queryByText(/^Organisation:/)).toBeNull();
      expect(screen.getByRole('link', { name: 'Organisation' }).getAttribute('href')).toBe('/organisation');
    });

    it('shows the organisation display as soon as the first membership is read', async () => {
      const { fixture, store } = await renderNav('bar', true);

      store.organizations.set(ONE_ORGANIZATION);
      await fixture.whenStable();

      expect(screen.getByText('Organisation: Ämter Musterhausen')).toBeTruthy();
    });
  });

  describe('organisation switch (R-SHL-03; S5-18)', () => {
    it('is a menu button named by its visible text whose menu lists every organisation, the active one checked', async () => {
      const { fixture } = await renderNav('bar', true, TEST_ORGANIZATIONS);

      screen.getByRole('button', { name: 'Organisation: Ämter Musterhausen' }).click();
      await fixture.whenStable();

      const menu = screen.getByRole('menu', { name: 'Organisation wechseln' });
      const options = within(menu).getAllByRole('menuitemradio');
      expect(options.map((option) => [option.textContent?.replace('check', '').trim(), option.getAttribute('aria-checked')])).toEqual([
        ['Ämter Musterhausen', 'true'],
        ['Verein Beispiel', 'false'],
      ]);
    });

    it('switches to the chosen organisation, which the display and „Organisation“ then name', async () => {
      const { fixture, store, navigated } = await renderNav('bar', true, TEST_ORGANIZATIONS);

      screen.getByRole('button', { name: 'Organisation: Ämter Musterhausen' }).click();
      await fixture.whenStable();
      screen.getByRole('menuitemradio', { name: /Verein Beispiel/ }).click();
      await fixture.whenStable();

      expect(store.switchOrganization).toHaveBeenCalledExactlyOnceWith('org-v');
      expect(navigated).toHaveBeenCalled();
      expect(screen.getByRole('button', { name: 'Organisation: Verein Beispiel' })).toBeTruthy();
      const menu = await openAccountMenu(fixture);
      expect(within(menu).getByRole('menuitem', { name: 'Organisation' }).getAttribute('href')).toBe('/organisation/org-v/mitglieder');
    });

    it('opens S21 of the chosen organisation when it switches on S21', async () => {
      const { fixture } = await renderNav('bar', true, TEST_ORGANIZATIONS);
      const router = TestBed.inject(Router);
      await router.navigateByUrl('/organisation/org-a/mitglieder');

      screen.getByRole('button', { name: 'Organisation: Ämter Musterhausen' }).click();
      await fixture.whenStable();
      screen.getByRole('menuitemradio', { name: /Verein Beispiel/ }).click();
      await fixture.whenStable();

      expect(router.url).toBe('/organisation/org-v/mitglieder');
    });

    it('stays on any other page when it switches', async () => {
      const { fixture } = await renderNav('bar', true, TEST_ORGANIZATIONS);
      const router = TestBed.inject(Router);
      await router.navigateByUrl('/products');

      screen.getByRole('button', { name: 'Organisation: Ämter Musterhausen' }).click();
      await fixture.whenStable();
      screen.getByRole('menuitemradio', { name: /Verein Beispiel/ }).click();
      await fixture.whenStable();

      expect(router.url).toBe('/products');
    });

    it('changes nothing when the active organisation is chosen again', async () => {
      const { fixture, store } = await renderNav('bar', true, TEST_ORGANIZATIONS);

      screen.getByRole('button', { name: 'Organisation: Ämter Musterhausen' }).click();
      await fixture.whenStable();
      screen.getByRole('menuitemradio', { name: /Ämter Musterhausen/ }).click();
      await fixture.whenStable();

      expect(store.switchOrganization).not.toHaveBeenCalled();
    });

    it('is a list of buttons in the compact menu, the active one marked current', async () => {
      const { fixture, store, navigated } = await renderNav('drawer', true, TEST_ORGANIZATIONS);

      expect(screen.getByText('Organisation: Ämter Musterhausen')).toBeTruthy();
      const list = screen.getByRole('list', { name: 'Organisation wechseln' });
      const buttons = within(list).getAllByRole('button');
      expect(buttons.map((button) => [button.textContent?.trim(), button.getAttribute('aria-current')])).toEqual([
        ['Ämter Musterhausen', 'true'],
        ['Verein Beispiel', null],
      ]);

      buttons[1].click();
      await fixture.whenStable();

      expect(store.switchOrganization).toHaveBeenCalledExactlyOnceWith('org-v');
      expect(navigated).toHaveBeenCalled();
      expect(screen.getByText('Organisation: Verein Beispiel')).toBeTruthy();
    });
  });

  // The session state is a signal (withIdentity(), S5-08): the nav follows it without a navigation,
  // also after an account deletion or a session that ended.
  it('shows „Anmelden“ again as soon as the session is gone', async () => {
    const { fixture, store } = await renderNav('bar', true, ONE_ORGANIZATION);
    expect(screen.getByRole('button', { name: /^Konto/ })).toBeTruthy();

    store.signedIn.set(false);
    await fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: /^Konto/ })).toBeNull();
  });

  it('ends the session, opens the start page and focuses its heading when „Abmelden“ is chosen (R-AUTH-08)', async () => {
    const { fixture, store } = await renderNav('bar', true);
    await TestBed.inject(Router).navigateByUrl('/products');
    const focusPageHeading = vi.spyOn(TestBed.inject(PageTitleStrategy), 'focusPageHeading').mockImplementation(() => undefined);

    const menu = await openAccountMenu(fixture);
    within(menu).getByRole('menuitem', { name: 'Abmelden' }).click();
    await fixture.whenStable();

    expect(store.endSession).toHaveBeenCalledOnce();
    expect(TestBed.inject(Router).url).toBe('/');
    expect(focusPageHeading).toHaveBeenCalledOnce();
  });

  it('opens the start page also when the sign-out request fails', async () => {
    const { fixture, store } = await renderNav('drawer', true);
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
