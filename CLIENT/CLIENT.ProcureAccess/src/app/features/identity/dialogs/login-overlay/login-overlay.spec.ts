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
import { AuthService } from '../../services/auth.service';
import { LoginOverlay } from './login-overlay';

const base = 'https://procureaccess.de/problems/';

describe('LoginOverlay (login overlay after 401, R-AUTH-07)', () => {
  let startSession: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;

  async function open(signIn: { rejectWith?: unknown } = {}) {
    startSession =
      signIn.rejectWith === undefined ? vi.fn().mockResolvedValue(undefined) : vi.fn().mockRejectedValue(signIn.rejectWith);
    announce = vi.fn().mockResolvedValue(undefined);
    close = vi.fn();
    const rendered = await render(LoginOverlay, {
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { startSession } },
        { provide: AuthService, useValue: { forgotPassword: vi.fn() } },
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

  it('explains the ended session and shows the S01 form with „Schließen“ before „Anmelden“', async () => {
    await open();

    expect(screen.getByRole('heading', { level: 2, name: 'Sie wurden abgemeldet' })).toBeTruthy();
    expect(screen.getByText(/Ihre Sitzung ist aus Sicherheitsgründen abgelaufen/)).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').id).toBe('pa-login-overlay-email');
    expect(screen.getByRole('button', { name: 'Passwort anzeigen' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Passwort vergessen?' })).toBeTruthy();
    const buttons = screen.getAllByRole('button').map((button) => button.textContent?.trim());
    expect(buttons.indexOf('Schließen')).toBeLessThan(buttons.indexOf('Anmelden'));
    await expectNoAxeViolations();
  });

  it('SignIn_Succeeds_AnnouncesRestoredAndClosesWithTrue INV:SEC-03', async () => {
    const { fixture } = await open();

    await fillAndSubmit(fixture);

    expect(startSession).toHaveBeenCalledWith('mod@example.test', 'secret');
    expect(announce).toHaveBeenCalledWith('shell.session.restored');
    expect(close).toHaveBeenCalledWith(true);
  });

  it('SignIn_Refused_ShowsErrorSummaryAndStaysOpen INV:SEC-03', async () => {
    const refused = new HttpErrorResponse({
      status: 400,
      error: { type: `${base}validation`, errors: { $: ['wrong'] } },
    });
    const { fixture } = await open({ rejectWith: refused });

    await fillAndSubmit(fixture);

    const summary = screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ }).closest('pa-notice')!;
    expect(summary.textContent).toContain('Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht.');
    expect(document.activeElement).toBe(summary);
    expect(close).not.toHaveBeenCalled();
    expect(announce).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });
});
