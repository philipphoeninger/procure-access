// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, RouterOutlet, TitleStrategy } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { NEW_PASSWORD_PATH } from '../../models/login-navigation';
import { PASSWORD_RULE } from '../../models/password-rule';
import { AuthService } from '../../services/auth.service';
import { NEW_PASSWORD_ROUTES } from './new-password.routing';

@Component({
  template: '<h1>Startseite</h1>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class HomePage {}

@Component({
  imports: [RouterOutlet],
  template: '<main id="main"><router-outlet /><router-outlet name="login" /></main>',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
class Host {}

const base = 'https://procureaccess.de/problems/';
const goodPassword = 'neues-passwort-12';

describe('New-password step of S03 (R-AUTH-04, /passwort/neu)', () => {
  let completePasswordReset: ReturnType<typeof vi.fn>;
  let announcePageTitle: ReturnType<typeof vi.fn>;

  async function openLink(url: string, answer: { rejectWith?: unknown } = {}) {
    completePasswordReset =
      answer.rejectWith === undefined ? vi.fn().mockResolvedValue(undefined) : vi.fn().mockRejectedValue(answer.rejectWith);
    announcePageTitle = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Host, {
      routes: [
        { path: '', component: HomePage },
        { path: 'passwort', component: HomePage },
        { path: NEW_PASSWORD_PATH, children: NEW_PASSWORD_ROUTES },
      ],
      providers: [
        provideTestTranslations(),
        { provide: TitleStrategy, useExisting: PageTitleStrategy },
        { provide: AuthService, useValue: { completePasswordReset } },
        { provide: PASSWORD_RULE, useValue: { minLength: 12, maxLength: 128 } },
        { provide: A11yAnnouncerService, useValue: { announce: vi.fn(), announcePageTitle } },
      ],
    });
    const router = TestBed.inject(Router);
    await router.navigateByUrl(url);
    await settle(rendered.fixture);
    return { ...rendered, router, location: TestBed.inject(Location) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  async function save(fixture: { whenStable(): Promise<unknown> }, password?: string) {
    if (password !== undefined) fireEvent.input(screen.getByLabelText('Neues Passwort'), { target: { value: password } });
    screen.getByRole('button', { name: 'Passwort speichern' }).click();
    await settle(fixture);
  }

  it('takes the token out of the address and shows the form with the hint of the 🔧 minimum', async () => {
    const { router, location } = await openLink('/passwort/neu?token=abc_DEF-123');

    expect(router.url).toBe('/passwort/neu');
    expect(location.path()).toBe('/passwort/neu');
    expect(screen.getByRole('heading', { level: 1, name: 'Passwort zurücksetzen' })).toBeTruthy();
    expect(screen.getByText('Mindestens 12 Zeichen.')).toBeTruthy();
    expect(screen.getByLabelText('Neues Passwort').getAttribute('autocomplete')).toBe('new-password');
    expect(document.title).toBe('Passwort zurücksetzen – ProcureAccess');
    expect(completePasswordReset).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('shows the password when „Passwort anzeigen“ is pressed', async () => {
    const { fixture } = await openLink('/passwort/neu?token=t1');
    const toggle = screen.getByRole('button', { name: 'Passwort anzeigen' });

    toggle.click();
    await fixture.whenStable();

    expect(toggle.getAttribute('aria-pressed')).toBe('true');
    expect(screen.getByLabelText('Neues Passwort').getAttribute('type')).toBe('text');
  });

  it('refuses a missing and a too short password with the count before sending anything', async () => {
    const { fixture } = await openLink('/passwort/neu?token=t1');

    await save(fixture);
    expect(screen.getByRole('link', { name: 'Neues Passwort: Bitte geben Sie Neues Passwort an.' })).toBeTruthy();

    await save(fixture, 'zu-kurz');
    expect(screen.getAllByText('Bitte geben Sie mindestens 12 Zeichen ein. Derzeit sind es 7.').length).toBeGreaterThan(0);
    expect(completePasswordReset).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('saves the password with the token and replaces the form with the focused result and „Zur Anmeldung“', async () => {
    const { fixture } = await openLink('/passwort/neu?token=t1');

    await save(fixture, goodPassword);

    expect(completePasswordReset).toHaveBeenCalledExactlyOnceWith('t1', goodPassword);
    const heading = screen.getByRole('heading', { level: 2, name: 'Passwort geändert' });
    expect(document.activeElement).toBe(heading);
    expect(screen.getByText('Ihr Passwort wurde geändert. Sie können sich jetzt anmelden.')).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Zur Anmeldung' }).getAttribute('href')).toBe('/(login:anmelden)');
    expect(screen.queryByLabelText('Neues Passwort')).toBeNull();
    await expectNoAxeViolations();
  });

  it('keeps the form when the server refuses the password, with its message at the field (API-02)', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { newPassword: ['Passwords must be at least 12 characters.'] } },
    });
    const { fixture } = await openLink('/passwort/neu?token=t1', { rejectWith: refused });

    await save(fixture, goodPassword);

    expect(screen.getByRole('link', { name: 'Neues Passwort: Passwords must be at least 12 characters.' })).toBeTruthy();
    expect(screen.getByLabelText('Neues Passwort').getAttribute('aria-invalid')).toBe('true');
  });

  it('shows „Link nicht mehr gültig“ with its title, focus and announcement when the server refuses the token', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { token: ['The link is unknown, expired or already used.'] } },
    });
    const { fixture } = await openLink('/passwort/neu?token=t1', { rejectWith: refused });

    await save(fixture, goodPassword);
    await settle(fixture);

    const heading = screen.getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' });
    expect(document.activeElement).toBe(heading);
    expect(document.title).toBe('Link nicht mehr gültig – ProcureAccess');
    expect(announcePageTitle).toHaveBeenLastCalledWith('Link nicht mehr gültig – ProcureAccess');
    expect(screen.getByRole('link', { name: 'Neuen Link anfordern' }).getAttribute('href')).toBe('/passwort');
    await expectNoAxeViolations();
  });

  it('shows a missing antiforgery token in the error summary and keeps the form', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}antiforgery` } });
    const { fixture } = await openLink('/passwort/neu?token=t1', { rejectWith: refused });

    await save(fixture, goodPassword);

    expect(screen.getByText(/aus Sicherheitsgründen nicht ausgeführt/)).toBeTruthy();
    expect(screen.getByLabelText('Neues Passwort')).toBeTruthy();
  });

  it('shows „Link nicht mehr gültig“ without a token in memory (a reload of the page)', async () => {
    await openLink('/passwort/neu');

    expect(screen.getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeTruthy();
    expect(screen.getByText('Dieser Link zum Zurücksetzen ist nicht mehr gültig. Bitte fordern Sie einen neuen an.')).toBeTruthy();
    expect(document.title).toBe('Link nicht mehr gültig – ProcureAccess');
    expect(screen.queryByLabelText('Neues Passwort')).toBeNull();
    await expectNoAxeViolations();
  });

  it('takes a second link opened on the page, after a refused one', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { token: ['used'] } },
    });
    const { fixture, router } = await openLink('/passwort/neu?token=t1', { rejectWith: refused });
    await save(fixture, goodPassword);
    completePasswordReset.mockResolvedValue(undefined);

    await router.navigateByUrl('/passwort/neu?token=t2');
    await settle(fixture);
    expect(document.title).toBe('Passwort zurücksetzen – ProcureAccess');
    expect(announcePageTitle).toHaveBeenLastCalledWith('Passwort zurücksetzen – ProcureAccess');
    expect(screen.getByLabelText<HTMLInputElement>('Neues Passwort').value).toBe('');
    await save(fixture, goodPassword);

    expect(completePasswordReset).toHaveBeenLastCalledWith('t2', goodPassword);
    expect(screen.getByRole('heading', { level: 2, name: 'Passwort geändert' })).toBeTruthy();
  });
});
