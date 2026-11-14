// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { render, screen } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ConfirmRequiredNotice } from './confirm-required-notice';

const base = 'https://procureaccess.de/problems/';

describe('ConfirmRequiredNotice (auth.confirm.required, R-AUTH-02)', () => {
  let resendConfirmation: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function renderNotice(answer: { rejectWith?: unknown; pending?: Promise<void> } = {}) {
    resendConfirmation = answer.pending
      ? vi.fn().mockReturnValue(answer.pending)
      : answer.rejectWith === undefined
        ? vi.fn().mockResolvedValue(undefined)
        : vi.fn().mockRejectedValue(answer.rejectWith);
    announce = vi.fn().mockResolvedValue(undefined);
    return render(ConfirmRequiredNotice, {
      providers: [
        provideTestTranslations(),
        { provide: ProcureAccessStore, useValue: { resendConfirmation, session: () => ({ email: 'ada@example.test' }) } },
        { provide: A11yAnnouncerService, useValue: { announce } },
      ],
    });
  }

  async function resend(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('INV:SEC-10 says why the action is blocked and offers to send the e-mail again', async () => {
    await renderNotice();

    expect(screen.getByText(/Achtung/)).toBeTruthy();
    expect(screen.getByText(/Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse\. Danach können Sie Listen speichern/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('INV:SEC-10 sends the e-mail again and shows and announces the address it went to', async () => {
    const { fixture } = await renderNotice();

    await resend(fixture);

    expect(resendConfirmation).toHaveBeenCalledOnce();
    expect(screen.getByText('Wir haben die E-Mail erneut an ada@example.test geschickt.')).toBeTruthy();
    expect(announce).toHaveBeenCalledWith('auth.confirm.sent', { email: 'ada@example.test' });
    await expectNoAxeViolations();
  });

  it('keeps the label with a spinner and aria-disabled while sending, and sends once', async () => {
    let finish!: () => void;
    const { fixture } = await renderNotice({ pending: new Promise<void>((resolve) => (finish = resolve)) });
    const button = screen.getByRole('button', { name: 'Bestätigungs-E-Mail erneut senden' });

    button.click();
    await fixture.whenStable();
    button.click();
    await fixture.whenStable();

    expect(button.getAttribute('aria-disabled')).toBe('true');
    expect(resendConfirmation).toHaveBeenCalledOnce();
    finish();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
    expect(button.getAttribute('aria-disabled')).toBeNull();
  });

  it('shows errors.email.sendFailed when the server could not send it (503 email-delivery)', async () => {
    const failed = new HttpErrorResponse({ status: 503, error: { type: `${base}email-delivery` } });
    const { fixture } = await renderNotice({ rejectWith: failed });

    await resend(fixture);

    expect(screen.getByText('Die E-Mail konnte gerade nicht versendet werden. Bitte versuchen Sie es in einigen Minuten erneut.')).toBeTruthy();
    expect(announce).toHaveBeenCalledWith('errors.email.sendFailed', undefined);
  });

  it('shows the rate limit with its minutes (429)', async () => {
    const limited = new HttpErrorResponse({ status: 429, error: { type: `${base}rate-limit`, retryAfterMinutes: 42 } });
    const { fixture } = await renderNotice({ rejectWith: limited });

    await resend(fixture);

    expect(screen.getByText(/Bitte warten Sie 42 Minuten und versuchen Sie es dann erneut/)).toBeTruthy();
  });

  it('leaves any other failure to the error interceptor', async () => {
    const { fixture } = await renderNotice({ rejectWith: new HttpErrorResponse({ status: 500 }) });

    await resend(fixture);

    expect(screen.queryByText(/geschickt|versendet|warten/)).toBeNull();
    expect(announce).not.toHaveBeenCalled();
  });
});
