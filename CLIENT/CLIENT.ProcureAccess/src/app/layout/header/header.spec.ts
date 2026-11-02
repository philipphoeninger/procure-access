// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { createShellDoubles, provideLayout, SHELL_TEST_ROUTES } from '@app/testing/shell-test-doubles';
import { Header } from './header';

async function renderHeader(wide: boolean, menuOpen = false) {
  const menuToggle = vi.fn();
  const { fixture } = await render(Header, {
    inputs: { menuOpen },
    on: { menuToggle },
    routes: SHELL_TEST_ROUTES,
    providers: [provideTestTranslations(), provideLayout(wide), ...createShellDoubles().providers],
  });
  return { fixture, menuToggle };
}

describe('Header', () => {
  describe('from 600 px (pa-app-header)', () => {
    it('is the banner with the app name, the claim and the main navigation', async () => {
      await renderHeader(true);

      expect(screen.getByRole('banner')).toBeTruthy();
      const home = screen.getByRole('link', { name: 'ProcureAccess Barrierefreie Software beschaffen' });
      expect(home.getAttribute('href')).toBe('/home');
      expect(screen.getByRole('navigation', { name: 'Hauptnavigation' })).toBeTruthy();
      expect(screen.queryByRole('button', { name: 'Menü' })).toBeNull();
    });

    // #120: the legacy header had English names („Search icon-button“ …) and an h1.
    it('has no heading and no icon-only controls', async () => {
      await renderHeader(true);

      expect(screen.queryByRole('heading')).toBeNull();
      for (const control of [...screen.getAllByRole('link'), ...screen.getAllByRole('button')]) {
        expect(control.textContent?.trim()).not.toBe('');
        expect(control.getAttribute('aria-label')).toBeNull();
      }
    });
  });

  describe('below 600 px (pa-app-header-compact)', () => {
    it('shows the app name and „Menü“ instead of the navigation', async () => {
      await renderHeader(false);

      expect(screen.getByRole('link', { name: 'ProcureAccess' }).getAttribute('href')).toBe('/home');
      expect(screen.queryByRole('navigation')).toBeNull();
      const menu = screen.getByRole('button', { name: 'Menü' });
      expect(menu.getAttribute('aria-controls')).toBe('main-nav');
      expect(menu.getAttribute('aria-expanded')).toBe('false');
    });

    it('reports „Menü“ to the shell', async () => {
      const { fixture, menuToggle } = await renderHeader(false);

      screen.getByRole('button', { name: 'Menü' }).click();
      await fixture.whenStable();

      expect(menuToggle).toHaveBeenCalledOnce();
    });

    it('shows the open menu with aria-expanded="true"', async () => {
      await renderHeader(false, true);

      expect(screen.getByRole('button', { name: 'Menü' }).getAttribute('aria-expanded')).toBe('true');
    });
  });
});
