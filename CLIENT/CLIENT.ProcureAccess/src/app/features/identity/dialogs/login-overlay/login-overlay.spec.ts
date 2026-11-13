// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MatDialogRef } from '@angular/material/dialog';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LoginOverlay } from './login-overlay';

const base = 'https://procureaccess.de/problems/';

describe('LoginOverlay (login overlay after 401, R-AUTH-07)', () => {
  let signIn: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;

  async function open(signInResult: ReturnType<typeof vi.fn> = vi.fn().mockResolvedValue(null)) {
    signIn = signInResult;
    announce = vi.fn().mockResolvedValue(undefined);
    close = vi.fn();
    const rendered = await render(LoginOverlay, {
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { signIn } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MatDialogRef, useValue: { close } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  async function fillAndSubmit(fixture: { whenStable(): Promise<unknown> }) {
    fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: 'mod@example.test' } });
    fireEvent.input(screen.getByLabelText('Passwort'), { target: { value: 'secret' } });
    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();
  }

  it('explains the ended session and offers the sign-in fields and „Schließen“', async () => {
    await open();

    expect(screen.getByRole('heading', { level: 2, name: 'Sie wurden abgemeldet' })).toBeTruthy();
    expect(screen.getByText(/Ihre Sitzung ist aus Sicherheitsgründen abgelaufen/)).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('autocomplete')).toBe('email');
    expect(screen.getByLabelText('Passwort').getAttribute('autocomplete')).toBe('current-password');
    expect(screen.getByRole('button', { name: 'Schließen' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('asks for both fields before it signs in', async () => {
    const { fixture } = await open();

    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();

    expect(signIn).not.toHaveBeenCalled();
    expect(screen.getByText('Bitte geben Sie E-Mail-Adresse an.')).toBeTruthy();
    expect(screen.getByText('Bitte geben Sie Passwort an.')).toBeTruthy();
  });

  it('SignIn_Succeeds_AnnouncesRestoredAndClosesWithTrue INV:SEC-03', async () => {
    const { fixture } = await open();

    await fillAndSubmit(fixture);

    expect(signIn).toHaveBeenCalledWith('mod@example.test', 'secret');
    expect(announce).toHaveBeenCalledWith('shell.session.restored');
    expect(close).toHaveBeenCalledWith(true);
  });

  it('shows „Anmeldung nicht möglich“ for a refused sign-in and keeps the dialog open', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { $: ['wrong'] } },
    });
    const { fixture } = await open(vi.fn().mockRejectedValue(refused));

    await fillAndSubmit(fixture);

    const alert = screen.getByRole('alert');
    expect(alert.textContent).toContain('Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht.');
    expect(close).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('shows the antiforgery text when the token was missing', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}antiforgery` } });
    const { fixture } = await open(vi.fn().mockRejectedValue(refused));

    await fillAndSubmit(fixture);

    expect(screen.getByRole('alert').textContent).toContain('aus Sicherheitsgründen nicht ausgeführt');
  });
});
