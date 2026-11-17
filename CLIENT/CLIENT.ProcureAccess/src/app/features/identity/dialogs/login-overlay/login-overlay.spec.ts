// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injector, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material/dialog';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { AuthService } from '../../services/auth.service';
import { SignOutService } from '../../services/sign-out.service';
import { LoginOverlay, LoginOverlayData } from './login-overlay';

const base = 'https://procureaccess.de/problems/';

describe('LoginOverlay (login overlay after 401, R-AUTH-07)', () => {
  let startSession: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let close: ReturnType<typeof vi.fn>;
  let refreshSession: ReturnType<typeof vi.fn>;
  let anotherUserSignedIn: ReturnType<typeof vi.fn>;
  let session: ReturnType<typeof signal<{ id: string } | null>>;

  /**
   * Opens the overlay over the session of `before` (null: nobody); the sign-in ends with the
   * session of `after`. `data` as the session warning passes it.
   */
  async function open(
    signIn: { rejectWith?: unknown; before?: string | null; after?: string; data?: LoginOverlayData } = {},
  ) {
    session = signal(signIn.before === undefined || signIn.before === null ? null : { id: signIn.before });
    const after = signIn.after ?? signIn.before ?? 'u1';
    startSession =
      signIn.rejectWith === undefined
        ? vi.fn(() => {
            session.set({ id: after });
            return Promise.resolve();
          })
        : vi.fn().mockRejectedValue(signIn.rejectWith);
    announce = vi.fn().mockResolvedValue(undefined);
    close = vi.fn();
    refreshSession = vi.fn().mockResolvedValue('signedOut');
    anotherUserSignedIn = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(LoginOverlay, {
      routes: [],
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { startSession, session, refreshSession } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MatDialogRef, useValue: { close } },
        { provide: SignOutService, useValue: { anotherUserSignedIn } },
        ...(signIn.data === undefined ? [] : [{ provide: MAT_DIALOG_DATA, useValue: signIn.data }]),
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
    expect(screen.getByRole('link', { name: 'Passwort vergessen? (öffnet in neuem Tab)' }).getAttribute('target')).toBe('_blank');
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

  it('the same account after a 401 retries: announces restored and closes with true INV:SEC-03', async () => {
    const { fixture } = await open({ before: 'u1', after: 'u1' });

    await fillAndSubmit(fixture);

    expect(announce).toHaveBeenCalledWith('shell.session.restored');
    expect(close).toHaveBeenCalledWith(true);
    expect(anotherUserSignedIn).not.toHaveBeenCalled();
  });

  it('AnotherAccount_SignsIn_NoRetryAndThePreviousUsersDataGoes INV:SEC-03', async () => {
    const { fixture } = await open({ before: 'u1', after: 'u2' });

    await fillAndSubmit(fixture);

    expect(close).toHaveBeenCalledWith(false);
    expect(anotherUserSignedIn).toHaveBeenCalledOnce();
    expect(announce).not.toHaveBeenCalled();
  });

  it('compares with the user the session warning passes, whose session its probe already cleared (D1 (b), D3 (b))', async () => {
    const { fixture } = await open({ before: null, after: 'u2', data: { previousUserId: 'u1' } });

    await fillAndSubmit(fixture);

    expect(close).toHaveBeenCalledWith(false);
    expect(anotherUserSignedIn).toHaveBeenCalledOnce();
  });

  it('closed without a sign-in, it reads the session again, so the header shows the user signed out (R-AUTH-07)', async () => {
    const { fixture } = await open({ before: 'u1' });

    fixture.destroy();

    expect(refreshSession).toHaveBeenCalledOnce();
  });

  it('reads nothing again after a sign-in', async () => {
    const { fixture } = await open({ before: 'u1' });
    await fillAndSubmit(fixture);

    fixture.destroy();

    expect(refreshSession).not.toHaveBeenCalled();
  });
});

describe('LoginOverlay.open (S5-30)', () => {
  it('calls the probe for a new antiforgery token first, without applying it, and passes the user of the ended session', async () => {
    const calls: string[] = [];
    const session = vi.fn(() => {
      calls.push('probe');
      return Promise.reject(new Error('offline'));
    });
    const dialogOpen = vi.fn(() => calls.push('open'));
    const refreshSession = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { session } },
        { provide: ProcureAccessStore, useValue: { session: signal({ id: 'u1' }), refreshSession } },
      ],
    });

    await LoginOverlay.open({ open: dialogOpen } as unknown as MatDialog, TestBed.inject(Injector));

    expect(calls).toEqual(['probe', 'open']);
    expect(refreshSession).not.toHaveBeenCalled();
    expect(dialogOpen).toHaveBeenCalledWith(LoginOverlay, expect.objectContaining({ data: { previousUserId: 'u1' } }));
  });

  it('keeps the user the session warning passes', async () => {
    const dialogOpen = vi.fn();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthService, useValue: { session: vi.fn().mockResolvedValue({ user: null }) } },
        { provide: ProcureAccessStore, useValue: { session: signal(null) } },
      ],
    });

    await LoginOverlay.open({ open: dialogOpen } as unknown as MatDialog, TestBed.inject(Injector), { previousUserId: 'u1' });

    expect(dialogOpen).toHaveBeenCalledWith(LoginOverlay, expect.objectContaining({ data: { previousUserId: 'u1' } }));
  });
});
