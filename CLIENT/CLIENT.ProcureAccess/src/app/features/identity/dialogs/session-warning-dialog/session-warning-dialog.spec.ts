// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, inject, signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { MatDialog } from '@angular/material/dialog';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { AuthService } from '../../services/auth.service';
import { SignOutService } from '../../services/sign-out.service';
import { SessionWarningDialog, SessionWarningKind } from './session-warning-dialog';

const kind = signal<SessionWarningKind>('idle');
const minutes = signal(5);

/** A page button, so the test sees where the focus goes; opens the dialog as the warning service does. */
@Component({ template: '<button type="button" (click)="open()">Seite</button>' })
class Opener {
  private readonly dialog = inject(MatDialog);

  open(): void {
    SessionWarningDialog.open(this.dialog, { kind: kind.asReadonly(), minutes: minutes.asReadonly() });
  }
}

describe('Session warning (R-AUTH-06; Handoff §5.3; A11Y-04)', () => {
  let extendSession: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let signOut: ReturnType<typeof vi.fn>;

  async function openWarning(warning: SessionWarningKind, extend = vi.fn().mockResolvedValue(undefined)) {
    kind.set(warning);
    minutes.set(5);
    extendSession = extend;
    announce = vi.fn().mockResolvedValue(undefined);
    signOut = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(Opener, {
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: AuthService, useValue: { extendSession } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: SignOutService, useValue: { signOut } },
      ],
    });
    const opener = screen.getByRole('button', { name: 'Seite' });
    opener.focus();
    opener.click();
    await settle(rendered.fixture);
    return { fixture: rendered.fixture, dialog: screen.getByRole('alertdialog', { name: 'Ihre Sitzung endet bald' }) };
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }): Promise<void> {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('Warning_IdleLimitNext_IsAnAlertDialogWithTheFocusOnStaySignedIn INV:A11Y-04', async () => {
    const { dialog } = await openWarning('idle');

    const text = 'Aus Sicherheitsgründen werden Sie in 5 Minuten abgemeldet. Danach melden Sie sich erneut an, bevor Sie weiterarbeiten. Ihre Eingaben auf dieser Seite bleiben erhalten.';
    expect(dialog.getAttribute('aria-describedby')).toBe('pa-session-warning-text');
    expect(document.getElementById('pa-session-warning-text')?.textContent?.trim()).toBe(text);
    const stay = within(dialog).getByRole('button', { name: 'Angemeldet bleiben' });
    expect(document.activeElement).toBe(stay);
    const buttons = within(dialog).getAllByRole('button').map((button) => button.textContent?.trim());
    expect(buttons).toEqual(['Jetzt abmelden', 'Angemeldet bleiben']);
    await expectNoAxeViolations();
  });

  it('Warning_AbsoluteLimitNext_AsksToSaveWithoutStayAndFocusesClose INV:A11Y-04', async () => {
    const { dialog } = await openWarning('absolute');

    expect(dialog.textContent).toContain(
      'Ihre Sitzung endet in 5 Minuten und kann nicht mehr verlängert werden. Speichern Sie Ihre Eingaben und melden Sie sich danach erneut an.',
    );
    expect(within(dialog).queryByRole('button', { name: 'Angemeldet bleiben' })).toBeNull();
    expect(document.activeElement).toBe(within(dialog).getByRole('button', { name: 'Schließen' }));
    const buttons = within(dialog).getAllByRole('button').map((button) => button.textContent?.trim());
    expect(buttons).toEqual(['Jetzt abmelden', 'Schließen']);
    await expectNoAxeViolations();
  });

  it('counts down to „1 Minute“ (singular) without a new announcement', async () => {
    const { fixture, dialog } = await openWarning('idle');

    minutes.set(1);
    await settle(fixture);

    expect(dialog.textContent).toContain('Aus Sicherheitsgründen werden Sie in 1 Minute abgemeldet.');
    expect(announce).not.toHaveBeenCalled();
  });

  it('StaySignedIn_Extends_AnnouncesAndCloses INV:A11Y-04', async () => {
    const { fixture, dialog } = await openWarning('idle');

    within(dialog).getByRole('button', { name: 'Angemeldet bleiben' }).click();
    await settle(fixture);

    expect(extendSession).toHaveBeenCalledOnce();
    expect(announce).toHaveBeenCalledWith('shell.session.extended');
    expect(screen.queryByRole('alertdialog')).toBeNull();
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Seite' }));
  });

  it('stays open when the extension fails (the error interceptor shows the failure)', async () => {
    const { fixture, dialog } = await openWarning('idle', vi.fn().mockRejectedValue(new Error('offline')));

    within(dialog).getByRole('button', { name: 'Angemeldet bleiben' }).click();
    await settle(fixture);

    expect(announce).not.toHaveBeenCalled();
    expect(screen.getByRole('alertdialog')).toBeTruthy();
  });

  it('„Jetzt abmelden“ closes the warning and signs out (R-AUTH-08)', async () => {
    const { fixture, dialog } = await openWarning('absolute');

    within(dialog).getByRole('button', { name: 'Jetzt abmelden' }).click();
    await settle(fixture);

    expect(screen.queryByRole('alertdialog')).toBeNull();
    expect(signOut).toHaveBeenCalledOnce();
    expect(extendSession).not.toHaveBeenCalled();
  });
});
