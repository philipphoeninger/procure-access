// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { ConnectionStatus } from '@app/core/errors/system-errors';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { createShellDoubles, provideLayout, SHELL_TEST_ROUTES } from '@app/testing/shell-test-doubles';
import { App } from './app';

async function renderShell(wide: boolean, doubles = createShellDoubles()) {
  const { fixture } = await render(App, {
    routes: SHELL_TEST_ROUTES,
    providers: [provideTestTranslations(), provideLayout(wide), ...doubles.providers],
  });
  return fixture;
}

const skipLink = () => screen.getByRole('link', { name: 'Zum Inhalt springen' });

describe('App shell (R-SHL-01)', () => {
  it('has the landmarks of Handoff §7 in order: banner, main navigation, main, contentinfo', async () => {
    await renderShell(true);

    const landmarks = [
      screen.getByRole('banner'),
      screen.getByRole('navigation', { name: 'Hauptnavigation' }),
      screen.getByRole('main'),
      screen.getByRole('contentinfo'),
    ];
    for (let i = 1; i < landmarks.length; i++) {
      // DOCUMENT_POSITION_FOLLOWING: the next landmark comes after the previous one.
      expect(landmarks[i - 1].compareDocumentPosition(landmarks[i]) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    }
    expect(screen.getByRole('main').id).toBe('main');
  });

  // Below 600 px the closed compact menu precedes the content in the DOM and is hidden by CSS,
  // which jsdom does not apply: the E2E shell test checks that case with the real Tab key.
  it('makes the skip link the first focusable element', async () => {
    const fixture = await renderShell(true);

    const focusable = (fixture.nativeElement as HTMLElement).querySelector(
      'a[href], button:not([disabled]), input, select, textarea, [tabindex]:not([tabindex="-1"])',
    );
    expect(focusable).toBe(skipLink());
  });

  it('moves the focus to main#main when the skip link is used', async () => {
    const fixture = await renderShell(true);

    skipLink().click();
    await fixture.whenStable();

    expect(document.activeElement).toBe(screen.getByRole('main'));
  });

  describe('compact menu (below 600 px)', () => {
    it('opens the main navigation with „Menü“ and closes it with „Menü schließen“', async () => {
      const fixture = await renderShell(false);
      const menu = screen.getByRole('button', { name: 'Menü' });

      menu.click();
      await fixture.whenStable();
      expect(menu.getAttribute('aria-expanded')).toBe('true');
      expect(screen.getByRole('navigation', { name: 'Hauptnavigation' })).toBeTruthy();

      screen.getByRole('button', { name: 'Menü schließen' }).click();
      await fixture.whenStable();
      expect(menu.getAttribute('aria-expanded')).toBe('false');
    });

    it('closes with Esc', async () => {
      const fixture = await renderShell(false);
      const menu = screen.getByRole('button', { name: 'Menü' });
      menu.click();
      await fixture.whenStable();

      fireEvent.keyDown(screen.getByRole('link', { name: 'Kriterien' }), { key: 'Escape', keyCode: 27 });
      await fixture.whenStable();

      expect(menu.getAttribute('aria-expanded')).toBe('false');
    });

    it('closes after an item was chosen', async () => {
      const fixture = await renderShell(false);
      const menu = screen.getByRole('button', { name: 'Menü' });
      menu.click();
      await fixture.whenStable();

      screen.getByRole('link', { name: 'Produkte' }).click();
      await fixture.whenStable();

      expect(menu.getAttribute('aria-expanded')).toBe('false');
    });
  });

  describe('system states at the top of main (Handoff §6.2, §6.5)', () => {
    it('shows the loading bar in main while the legacy store loads', async () => {
      const doubles = createShellDoubles();
      const fixture = await renderShell(true, doubles);
      expect(screen.queryByRole('progressbar')).toBeNull();

      doubles.store.isLoading.set(true);
      await fixture.whenStable();

      expect(screen.getByRole('main').contains(screen.getByRole('progressbar', { name: 'Wird geladen …' }))).toBe(true);
    });

    it('shows the offline notice as a status in main until the server answers again', async () => {
      const fixture = await renderShell(true);
      const connection = TestBed.inject(ConnectionStatus);
      expect(screen.queryByRole('status')).toBeNull();

      connection.noAnswer();
      await fixture.whenStable();
      const notice = screen.getByRole('status');
      expect(notice.textContent).toContain('Keine Verbindung zum Server. Bitte prüfen Sie Ihre Internetverbindung.');
      expect(screen.getByRole('main').firstElementChild?.nextElementSibling).toBe(notice);

      connection.answered();
      await fixture.whenStable();
      expect(screen.queryByRole('status')).toBeNull();
    });
  });
});
