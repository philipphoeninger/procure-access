// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MatDialog } from '@angular/material/dialog';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';
import { of } from 'rxjs';
import type { Mock } from 'vitest';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { AuthService } from '../../services/auth.service';
import { LoginForm } from './login-form';

const base = 'https://procureaccess.de/problems/';

describe('LoginForm (sign-in form of S01 and the login overlay, R-AUTH-03)', () => {
  let startSession: ReturnType<typeof vi.fn>;
  let signedIn: Mock<() => void>;
  let errorsShown: Mock<(shown: boolean) => void>;
  let forgotPassword: ReturnType<typeof vi.fn>;

  async function renderForm(signIn: { rejectWith?: unknown; pending?: Promise<void> } = {}) {
    startSession =
      signIn.rejectWith === undefined
        ? vi.fn().mockReturnValue(signIn.pending ?? Promise.resolve())
        : vi.fn().mockRejectedValue(signIn.rejectWith);
    signedIn = vi.fn<() => void>();
    errorsShown = vi.fn<(shown: boolean) => void>();
    forgotPassword = vi.fn().mockReturnValue(of(null));
    const rendered = await render(LoginForm, {
      inputs: { idPrefix: 'test-login' },
      on: { signedIn, errorsShown },
      providers: [
        provideTestTranslations(),
        {
          provide: ProcureAccessStore,
          useValue: { startSession, incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() },
        },
        { provide: AuthService, useValue: { forgotPassword } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Anmelden' }).click();
    await fixture.whenStable();
  }

  async function fillAndSubmit(fixture: { whenStable(): Promise<unknown> }) {
    fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: 'mod@example.test' } });
    fireEvent.input(screen.getByLabelText('Passwort'), { target: { value: 'secret' } });
    await submit(fixture);
  }

  const refusal = (type: string, errors?: Record<string, string[]>) =>
    new HttpErrorResponse({ status: 400, error: { type: `${base}${type}`, errors } });

  it('shows the required note, both fields with their autocomplete tokens and the actions', async () => {
    await renderForm();

    expect(screen.getByText('Alle Felder ohne „(optional)“ müssen ausgefüllt werden.')).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('autocomplete')).toBe('email');
    expect(screen.getByLabelText('Passwort').getAttribute('autocomplete')).toBe('current-password');
    expect(screen.getByRole('button', { name: 'Passwort anzeigen' }).getAttribute('aria-pressed')).toBe('false');
    expect(screen.getByRole('button', { name: 'Passwort vergessen?' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Anmelden' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('shows no field error before the first submit, also after leaving an empty field', async () => {
    const { fixture } = await renderForm();

    fireEvent.blur(screen.getByLabelText('E-Mail-Adresse'));
    await fixture.whenStable();

    expect(screen.queryByText('Bitte geben Sie E-Mail-Adresse an.')).toBeNull();
  });

  it('lists the empty fields in the error summary, which takes the focus, and does not sign in (Handoff §6.4)', async () => {
    const { fixture } = await renderForm();

    await submit(fixture);

    expect(startSession).not.toHaveBeenCalled();
    const summary = screen.getByRole('heading', { level: 2, name: /2 Angaben müssen noch korrigiert werden/ });
    expect(summary.closest('pa-notice')).toBe(document.activeElement);
    const emailLine = screen.getByRole('link', { name: 'E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an.' });
    expect(screen.getByRole('link', { name: 'Passwort: Bitte geben Sie Passwort an.' })).toBeTruthy();
    // Material leaves aria-invalid off an empty required field and links the error instead.
    const fieldError = screen.getByText('Bitte geben Sie E-Mail-Adresse an.');
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('aria-describedby')).toContain(fieldError.id);
    expect(errorsShown).toHaveBeenLastCalledWith(true);

    emailLine.click();
    expect(document.activeElement).toBe(screen.getByLabelText('E-Mail-Adresse'));
    await expectNoAxeViolations();
  });

  it('signs in with the typed values and reports it', async () => {
    const { fixture } = await renderForm();

    await fillAndSubmit(fixture);

    expect(startSession).toHaveBeenCalledWith('mod@example.test', 'secret');
    expect(signedIn).toHaveBeenCalledOnce();
    expect(errorsShown).toHaveBeenLastCalledWith(false);
  });

  it('shows „Anmeldung nicht möglich“ without naming a field for a refused sign-in (R-AUTH-03)', async () => {
    const { fixture } = await renderForm({ rejectWith: refusal('validation', { $: ['Invalid sign-in.'] }) });

    await fillAndSubmit(fixture);

    expect(signedIn).not.toHaveBeenCalled();
    const summary = screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ }).closest('pa-notice')!;
    expect(summary.textContent).toContain('Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht.');
    expect(summary.querySelector('a')).toBeNull();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('aria-invalid')).toBe('false');
    expect(screen.getByLabelText('Passwort').getAttribute('aria-invalid')).toBe('false');
    expect(document.activeElement).toBe(summary);
    await expectNoAxeViolations();
  });

  it('binds a server error of a field to that field and its summary line', async () => {
    const { fixture } = await renderForm({ rejectWith: refusal('validation', { email: ['Too long.'] }) });

    await fillAndSubmit(fixture);

    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: Too long.' })).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('aria-invalid')).toBe('true');
  });

  it('shows the antiforgery text when the token was missing', async () => {
    const { fixture } = await renderForm({ rejectWith: refusal('antiforgery') });

    await fillAndSubmit(fixture);

    expect(screen.getByText(/aus Sicherheitsgründen nicht ausgeführt/)).toBeTruthy();
  });

  it('leaves other failures to the error interceptor and keeps the form usable', async () => {
    const { fixture } = await renderForm({ rejectWith: new HttpErrorResponse({ status: 503 }) });

    await fillAndSubmit(fixture);

    expect(screen.queryByRole('heading', { level: 2 })).toBeNull();
    expect(screen.getByRole('button', { name: 'Anmelden' }).getAttribute('aria-disabled')).toBeNull();
  });

  it('marks „Anmelden“ busy while the sign-in runs and ignores a second submit', async () => {
    let finish!: () => void;
    const { fixture } = await renderForm({ pending: new Promise<void>((resolve) => (finish = resolve)) });

    await fillAndSubmit(fixture);
    const button = screen.getByRole('button', { name: 'Anmelden' });
    expect(button.getAttribute('aria-disabled')).toBe('true');
    await submit(fixture);
    expect(startSession).toHaveBeenCalledOnce();

    finish();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
    expect(button.getAttribute('aria-disabled')).toBeNull();
  });

  it('shows the password as text while „Passwort anzeigen“ is pressed', async () => {
    const { fixture } = await renderForm();
    const toggle = screen.getByRole('button', { name: 'Passwort anzeigen' });

    toggle.click();
    await fixture.whenStable();

    expect(toggle.getAttribute('aria-pressed')).toBe('true');
    expect(screen.getByLabelText('Passwort').getAttribute('type')).toBe('text');
    expect(toggle.getAttribute('aria-controls')).toBe('test-login-password');
  });

  it('opens the legacy reset dialog with the typed address and requests the link (until S03)', async () => {
    const { fixture } = await renderForm();
    const afterClosed = of('mod@example.test');
    const open = vi.spyOn(fixture.debugElement.injector.get(MatDialog), 'open').mockReturnValue({ afterClosed: () => afterClosed } as never);
    fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: 'mod@example.test' } });

    screen.getByRole('button', { name: 'Passwort vergessen?' }).click();
    await fixture.whenStable();

    expect(open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: { email: 'mod@example.test' } }));
    expect(forgotPassword).toHaveBeenCalledWith('mod@example.test');
  });
});
