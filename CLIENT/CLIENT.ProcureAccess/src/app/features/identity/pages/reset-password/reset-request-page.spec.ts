// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { render, screen } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { AuthService } from '../../services/auth.service';
import { ResetRequestPage } from './reset-request-page';

const base = 'https://procureaccess.de/problems/';

describe('ResetRequestPage (S03 request step, R-AUTH-04)', () => {
  let requestPasswordReset: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(options: { rejectWith?: unknown } = {}) {
    requestPasswordReset =
      options.rejectWith === undefined ? vi.fn().mockResolvedValue({ validHours: 2 }) : vi.fn().mockRejectedValue(options.rejectWith);
    setPageTitleState = vi.fn();
    return render(ResetRequestPage, {
      providers: [
        provideTestTranslations(),
        { provide: AuthService, useValue: { requestPasswordReset } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
      ],
    });
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  async function submit(fixture: { whenStable(): Promise<unknown> }, email?: string) {
    if (email !== undefined) fireEvent.input(screen.getByLabelText('E-Mail-Adresse'), { target: { value: email } });
    screen.getByRole('button', { name: 'Link anfordern' }).click();
    await settle(fixture);
  }

  it('shows the H1, the intro that reveals no account, the required note and the address field', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Passwort zurücksetzen' })).toBeTruthy();
    expect(screen.getByText(/Wenn ein Konto dazu existiert, erhalten Sie einen Link zum Zurücksetzen/)).toBeTruthy();
    expect(screen.getByText('Alle Felder ohne „(optional)“ müssen ausgefüllt werden.')).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('autocomplete')).toBe('email');
    await expectNoAxeViolations();
  });

  it('lists a missing address in the error summary, which takes the focus, and sends nothing', async () => {
    const { fixture } = await openPage();

    await submit(fixture);

    const summary = screen.getByRole('heading', { level: 2, name: /1 Angabe muss noch korrigiert werden/ }).closest('pa-notice');
    expect(document.activeElement).toBe(summary);
    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: Bitte geben Sie E-Mail-Adresse an.' }).getAttribute('href')).toBe('#pa-reset-email');
    expect(setPageTitleState).toHaveBeenLastCalledWith({ error: true });
    expect(requestPasswordReset).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('refuses an address that is not one', async () => {
    const { fixture } = await openPage();

    await submit(fixture, 'ada.example.test');

    expect(screen.getAllByText('Bitte geben Sie eine gültige E-Mail-Adresse ein, zum Beispiel name@beispiel.de.').length).toBeGreaterThan(0);
    expect(requestPasswordReset).not.toHaveBeenCalled();
  });

  it('asks for the link and replaces the form with the focused confirmation naming the address and the hours', async () => {
    const { fixture } = await openPage();

    await submit(fixture, ' ada@example.test ');

    expect(requestPasswordReset).toHaveBeenCalledWith('ada@example.test');
    const sent = screen.getByText('Falls ein Konto zu ada@example.test existiert, ist der Link unterwegs. Er ist 2 Stunden gültig.');
    expect(document.activeElement).toBe(sent);
    expect(screen.queryByRole('button', { name: 'Link anfordern' })).toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith({ error: false });
    await expectNoAxeViolations();
  });

  it('binds the server field error to the field (API-02)', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}validation`, errors: { email: ['The e-mail address is not valid.'] } } });
    const { fixture } = await openPage({ rejectWith: refused });

    await submit(fixture, 'ada@example.test');

    expect(screen.getByRole('link', { name: 'E-Mail-Adresse: The e-mail address is not valid.' })).toBeTruthy();
    expect(screen.getByLabelText('E-Mail-Adresse').getAttribute('aria-invalid')).toBe('true');
  });

  it('shows the rate limit with its minutes in the error summary (API-05)', async () => {
    const limited = new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 60 } });
    const { fixture } = await openPage({ rejectWith: limited });

    await submit(fixture, 'ada@example.test');

    expect(screen.getByText(/Bitte warten Sie 60 Minuten und versuchen Sie es dann erneut/)).toBeTruthy();
  });

  it('shows a missing antiforgery token in the error summary', async () => {
    const refused = new HttpErrorResponse({ status: 400, error: { type: `${base}antiforgery` } });
    const { fixture } = await openPage({ rejectWith: refused });

    await submit(fixture, 'ada@example.test');

    expect(screen.getByText(/aus Sicherheitsgründen nicht ausgeführt/)).toBeTruthy();
  });
});
