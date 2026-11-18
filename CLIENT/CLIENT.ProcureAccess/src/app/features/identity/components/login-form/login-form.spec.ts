// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';
import type { Mock } from 'vitest';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LOCKOUT_RULE } from '../../models/lockout-rule';
import { LoginForm } from './login-form';

const base = 'https://procureaccess.de/problems/';

describe('LoginForm (sign-in form of S01 and the login overlay, R-AUTH-03)', () => {
  let startSession: ReturnType<typeof vi.fn>;
  let signedIn: Mock<() => void>;
  let errorsShown: Mock<(shown: boolean) => void>;

  async function renderForm(signIn: { rejectWith?: unknown; pending?: Promise<void> } = {}, forgotInNewTab = false) {
    startSession =
      signIn.rejectWith === undefined
        ? vi.fn().mockReturnValue(signIn.pending ?? Promise.resolve())
        : vi.fn().mockRejectedValue(signIn.rejectWith);
    signedIn = vi.fn<() => void>();
    errorsShown = vi.fn<(shown: boolean) => void>();
    const rendered = await render(LoginForm, {
      inputs: { idPrefix: 'test-login', forgotInNewTab },
      routes: [],
      on: { signedIn, errorsShown },
      providers: [
        provideTestTranslations(),
        {
          provide: ProcureAccessStore,
          useValue: { startSession, incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() },
        },
        { provide: LOCKOUT_RULE, useValue: { maxFailedAttempts: 5, minutes: 15 } },
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
    expect(screen.getByRole('link', { name: 'Passwort vergessen?' })).toBeTruthy();
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

  it('shows „Anmeldung nicht möglich“ without naming a field, and the lockout note, for a refused sign-in (R-AUTH-03)', async () => {
    const { fixture } = await renderForm({ rejectWith: refusal('validation', { $: ['Invalid sign-in.'] }) });

    await fillAndSubmit(fixture);

    expect(signedIn).not.toHaveBeenCalled();
    // The note is not an error: the count stays 1 (owner decision S5-13b 2 (a))
    const summary = screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ }).closest('pa-notice')!;
    expect(summary.textContent).toContain('Anmeldung nicht möglich. E-Mail-Adresse oder Passwort stimmen nicht.');
    expect(summary.textContent).toContain('Nach 5 Fehlversuchen ist die Anmeldung aus Sicherheitsgründen für 15 Minuten gesperrt.');
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
    expect(screen.queryByText(/Fehlversuchen/)).toBeNull();
  });

  it('shows the antiforgery text when the token was missing', async () => {
    const { fixture } = await renderForm({ rejectWith: refusal('antiforgery') });

    await fillAndSubmit(fixture);

    expect(screen.getByText(/aus Sicherheitsgründen nicht ausgeführt/)).toBeTruthy();
  });

  it('shows the wait of a 429 in the error summary, without the lockout note (SPEC §7.3, S6-26)', async () => {
    const { fixture } = await renderForm({
      rejectWith: new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 60 } }),
    });

    await fillAndSubmit(fixture);

    expect(signedIn).not.toHaveBeenCalled();
    const summary = screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ }).closest('pa-notice')!;
    expect(summary.textContent).toContain('Bitte warten Sie 60 Minuten und versuchen Sie es dann erneut.');
    expect(summary.textContent).not.toContain('Fehlversuchen');
    expect(document.activeElement).toBe(summary);
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

  it('links „Passwort vergessen?“ to S03 in place of the page, closing the login outlet (S01)', async () => {
    await renderForm();

    const link = screen.getByRole('link', { name: 'Passwort vergessen?' });

    expect(link.getAttribute('href')).toBe('/passwort');
    expect(link.getAttribute('target')).toBeNull();
  });

  it('opens S03 in a new tab from the login overlay, so the page under it keeps its input (S5-13b 1 (a))', async () => {
    await renderForm({}, true);

    const link = screen.getByRole('link', { name: 'Passwort vergessen? (öffnet in neuem Tab)' });

    expect(link.getAttribute('href')).toBe('/passwort');
    expect(link.getAttribute('target')).toBe('_blank');
    await expectNoAxeViolations();
  });
});
