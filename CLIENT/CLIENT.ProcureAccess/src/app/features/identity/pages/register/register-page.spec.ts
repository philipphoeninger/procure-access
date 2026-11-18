// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { render, screen, within } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { PASSWORD_RULE } from '../../models/password-rule';
import { AuthService } from '../../services/auth.service';
import { RegisterPage } from './register-page';

const base = 'https://procureaccess.de/problems/';
const goodPassword = 'lang-genug-2026';

describe('RegisterPage (S02, R-AUTH-01)', () => {
  let register: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(options: { rejectWith?: unknown } = {}) {
    register = options.rejectWith === undefined ? vi.fn().mockResolvedValue(undefined) : vi.fn().mockRejectedValue(options.rejectWith);
    setPageTitleState = vi.fn();
    return render(RegisterPage, {
      routes: [],
      providers: [
        provideTestTranslations(),
        { provide: AuthService, useValue: { register } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: PASSWORD_RULE, useValue: { minLength: 12, maxLength: 128 } },
      ],
    });
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  function fill(values: { email?: string; password?: string; consent?: boolean }) {
    if (values.email !== undefined) fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: values.email } });
    if (values.password !== undefined) fireEvent.input(screen.getByLabelText('Passwort'), { target: { value: values.password } });
    if (values.consent) screen.getByRole('checkbox', { name: /^Ich habe die Datenschutzhinweise/ }).click();
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Konto erstellen' }).click();
    await settle(fixture);
  }

  it('shows the H1, the intro, the fields with the 🔧 minimum as hint and the privacy note', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Konto erstellen' })).toBeTruthy();
    expect(screen.getByText(/Zum Lesen von Kriterien und Produkten brauchen Sie kein Konto/)).toBeTruthy();
    expect(screen.getByText('Mindestens 12 Zeichen.')).toBeTruthy();
    expect(screen.getByText('Ihre E-Mail-Adresse wird nie öffentlich angezeigt.')).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('autocomplete')).toBe('email');
    expect(screen.getByLabelText('Passwort').getAttribute('autocomplete')).toBe('new-password');
    await expectNoAxeViolations();
  });

  it('links „Datenschutzhinweise“ inside the acknowledgement to the privacy page in a new tab', async () => {
    await openPage();

    const checkbox = screen.getByRole('checkbox', { name: 'Ich habe die Datenschutzhinweise (öffnet in neuem Tab) gelesen.' });
    const link = screen.getByRole('link', { name: 'Datenschutzhinweise (öffnet in neuem Tab)' });
    expect(link.getAttribute('href')).toBe('/datenschutz');
    expect(link.getAttribute('target')).toBe('_blank');
    expect((checkbox as HTMLInputElement).checked).toBe(false);
  });

  it('shows the password when „Passwort anzeigen“ is pressed', async () => {
    const { fixture } = await openPage();
    const toggle = screen.getByRole('button', { name: 'Passwort anzeigen' });

    toggle.click();
    await fixture.whenStable();

    expect(toggle.getAttribute('aria-pressed')).toBe('true');
    expect(screen.getByLabelText('Passwort').getAttribute('type')).toBe('text');
  });

  it('lists every missing field in the error summary, focuses it and prefixes the page title (Handoff §6.4)', async () => {
    const { fixture } = await openPage();

    await submit(fixture);

    const summary = screen.getByRole('heading', { level: 2, name: /3 Angaben müssen noch korrigiert werden/ });
    expect(summary).toBeTruthy();
    expect(screen.getByRole('link', { name: /E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an/ }).getAttribute('href')).toBe('#pa-register-email');
    expect(screen.getByRole('link', { name: /Passwort: Bitte geben Sie Passwort an/ }).getAttribute('href')).toBe('#pa-register-password');
    expect(
      screen.getByRole('link', { name: /Bitte bestätigen Sie, dass Sie die Datenschutzhinweise gelesen haben/ }).getAttribute('href'),
    ).toBe('#pa-register-consent-input');
    expect(screen.getByText('Bitte bestätigen Sie, dass Sie die Datenschutzhinweise gelesen haben.', { selector: 'p' }).id).toBe(
      'pa-register-consent-error',
    );
    expect(setPageTitleState).toHaveBeenLastCalledWith({ error: true });
    expect(register).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('refuses an address that is not one and a password shorter than the minimum, with the count (errors.field.minLength)', async () => {
    const { fixture } = await openPage();

    fill({ email: 'ada.example.test', password: 'zu-kurz', consent: true });
    await submit(fixture);

    expect(screen.getAllByText('Bitte geben Sie eine gültige E-Mail-Adresse ein, zum Beispiel name@beispiel.de.').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Bitte geben Sie mindestens 12 Zeichen ein. Derzeit sind es 7.').length).toBeGreaterThan(0);
    expect(screen.queryByText('Bitte bestätigen Sie, dass Sie die Datenschutzhinweise gelesen haben.', { selector: 'p' })).toBeNull();
    expect(register).not.toHaveBeenCalled();
  });

  it('refuses a password longer than 128 characters (errors.field.maxLength)', async () => {
    const { fixture } = await openPage();

    fill({ email: 'ada@example.test', password: 'x'.repeat(129), consent: true });
    await submit(fixture);

    expect(screen.getAllByText('Bitte kürzen Sie den Text auf höchstens 128 Zeichen. Derzeit sind es 129.').length).toBeGreaterThan(0);
    expect(register).not.toHaveBeenCalled();
  });

  it('clears the acknowledgement error when the box is ticked', async () => {
    const { fixture } = await openPage();
    await submit(fixture);

    fill({ consent: true });
    await fixture.whenStable();

    expect(screen.queryByText('Bitte bestätigen Sie, dass Sie die Datenschutzhinweise gelesen haben.', { selector: 'p' })).toBeNull();
  });

  it('sends the registration and replaces the form with the focused success heading and the address', async () => {
    const { fixture } = await openPage();

    fill({ email: ' ada@example.test ', password: goodPassword, consent: true });
    await submit(fixture);

    expect(register).toHaveBeenCalledWith({ email: 'ada@example.test', password: goodPassword, privacyAcknowledged: true });
    const heading = screen.getByRole('heading', { level: 2, name: 'E-Mail-Adresse bestätigen' });
    expect(document.activeElement).toBe(heading);
    expect(
      screen.getByText('Fast geschafft. Wir haben Ihnen eine E-Mail an ada@example.test geschickt. Bitte bestätigen Sie darin Ihre Adresse.'),
    ).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Konto erstellen' })).toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith({ error: false });
    await expectNoAxeViolations();
  });

  // API-02 moved here from the legacy product proposal (S2-11) before the proposals went (S6-23).
  // "Announced" (WCAG 3.3.1) means the focus on the error summary, not a live region (S2-11 decision 1).
  const emailError = 'The e-mail address is not valid.';
  const bodyError = 'The request body is invalid.';

  function validationProblem(errors: Record<string, string[]>) {
    return new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, title: 'Validation failed', status: 400, errors } });
  }

  it('Submit_ServerFieldErrors_BoundToFieldsAndSummaryFocused INV:API-02', async () => {
    const { fixture } = await openPage({ rejectWith: validationProblem({ email: [emailError], $: [bodyError] }) });

    fill({ email: 'ada@example.test', password: goodPassword, consent: true });
    await submit(fixture);

    // The summary names the number of errors and takes the focus, so it is announced.
    const heading = screen.getByRole('heading', { level: 2, name: 'Fehler: 2 Angaben müssen noch korrigiert werden' });
    const notice = heading.closest('pa-notice') as HTMLElement;
    expect(document.activeElement).toBe(notice);

    // The field error sits at the field and is linked to it.
    const email = screen.getByLabelText('E-Mail-Adresse');
    const fieldError = screen.getByText(emailError, { selector: 'mat-error' });
    expect(email.getAttribute('aria-invalid')).toBe('true');
    expect(email.getAttribute('aria-describedby')?.split(' ')).toContain(fieldError.id);

    // A known field is a link in the summary that moves the focus to the field;
    // a path without a field ($ = the whole body) is a line without a link.
    const summary = within(notice);
    summary.getByRole('link', { name: `E-Mail-Adresse: ${emailError}` }).click();
    expect(document.activeElement).toBe(email);
    expect(summary.getByText(bodyError).closest('a')).toBeNull();
    expect(summary.getAllByRole('link')).toHaveLength(1);
    expect(screen.queryByRole('heading', { name: 'E-Mail-Adresse bestätigen' })).toBeNull();
  });

  it('removes a server field error when the user edits the field and focuses the summary again on the next refusal', async () => {
    const { fixture } = await openPage({ rejectWith: validationProblem({ email: [emailError] }) });
    fill({ email: 'ada@example.test', password: goodPassword, consent: true });
    await submit(fixture);

    fill({ email: 'grace@example.test' });
    await fixture.whenStable();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('aria-invalid')).toBe('false');
    expect(screen.queryByText(emailError, { selector: 'mat-error' })).toBeNull();

    screen.getByLabelText('E-Mail-Adresse').focus();
    await submit(fixture);

    const heading = screen.getByRole('heading', { level: 2, name: 'Fehler: 1 Angabe muss noch korrigiert werden' });
    expect(document.activeElement).toBe(heading.closest('pa-notice'));
  });

  it('shows the rate limit with its minutes in the error summary (API-05)', async () => {
    const limited = new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 1 } });
    const { fixture } = await openPage({ rejectWith: limited });

    fill({ email: 'ada@example.test', password: goodPassword, consent: true });
    await submit(fixture);

    expect(screen.getByText(/Bitte warten Sie 1 Minute und versuchen Sie es dann erneut/)).toBeTruthy();
  });

  it('shows a missing antiforgery token in the error summary', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}antiforgery` } });
    const { fixture } = await openPage({ rejectWith: refused });

    fill({ email: 'ada@example.test', password: goodPassword, consent: true });
    await submit(fixture);

    expect(screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ })).toBeTruthy();
  });
});
