// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject, signal, Signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';
import { AuthService } from '../../services/auth.service';
import { SignOutService } from '../../services/sign-out.service';

/** Which limit is next: the idle limit, which „Angemeldet bleiben“ moves, or the absolute one, which nothing moves. */
export type SessionWarningKind = 'idle' | 'absolute';

/** The state the session warning service keeps current while the dialog is open. */
export interface SessionWarningData {
  kind: Signal<SessionWarningKind>;
  /** Whole minutes until the limit, rounded up; changes at most once a minute. */
  minutes: Signal<number>;
}

/** Id of the text that describes the alert dialog. */
export const SESSION_WARNING_TEXT_ID = 'pa-session-warning-text';

/**
 * The session warning (SPEC R-AUTH-06; Handoff §5.3 „Session warning“; UX copy §5.1
 * `shell.session.*`; A11Y-04, WCAG 2.2.1). An alert dialog, described by its text, which the
 * session warning service opens `warningLeadSeconds` before the earlier limit.
 *
 * - Idle limit next: `shell.session.warning.body`, „Jetzt abmelden“ and „Angemeldet bleiben“ with
 *   the focus. „Angemeldet bleiben“ calls `POST /auth/session/extend`, announces
 *   `shell.session.extended` and closes; a failure keeps the dialog open (the error interceptor
 *   shows it, a 401 opens the login overlay and sends the request again after the sign-in).
 * - Absolute limit next: `shell.session.warning.absolute` without „Angemeldet bleiben“, „Jetzt
 *   abmelden“ and „Schließen“ with the focus (owner decision S5-30 D2 (a)).
 *
 * The minutes change at most once a minute and are not announced again (UX copy). „Jetzt
 * abmelden“ signs out as „Abmelden“ in the menu „Konto“ does. Esc closes the dialog without
 * extending; any later request renews the session anyway.
 *
 * @spec docs/SPEC_v2.md R-AUTH-06, §8.8; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants A11Y-04
 */
@Component({
  selector: 'pa-session-warning-dialog',
  imports: [MatButtonModule, MatDialogModule, TranslatePipe, TranslatePluralPipe],
  templateUrl: './session-warning-dialog.html',
  styleUrl: './session-warning-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SessionWarningDialog {
  /** Opens the alert dialog with the focus on the action of `kind`. */
  static open(dialog: MatDialog, data: SessionWarningData): MatDialogRef<SessionWarningDialog> {
    return dialog.open<SessionWarningDialog, SessionWarningData>(SessionWarningDialog, {
      data,
      role: 'alertdialog',
      ariaDescribedBy: SESSION_WARNING_TEXT_ID,
      autoFocus: data.kind() === 'idle' ? '.pa-session-warning__stay' : '.pa-session-warning__close',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
  }

  private readonly data = inject<SessionWarningData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<SessionWarningDialog>>(MatDialogRef);
  private readonly authService = inject(AuthService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly signOutService = inject(SignOutService);

  protected readonly textId = SESSION_WARNING_TEXT_ID;
  protected readonly kind = this.data.kind;
  protected readonly minutes = this.data.minutes;
  private readonly busy = signal(false);

  protected async stay(): Promise<void> {
    if (this.busy()) {
      return;
    }
    this.busy.set(true);
    try {
      await this.authService.extendSession();
    } catch {
      // The error interceptor shows the failure; the warning stays.
      return;
    } finally {
      this.busy.set(false);
    }
    void this.announcer.announce('shell.session.extended');
    this.dialogRef.close();
  }

  protected logout(): void {
    this.dialogRef.close();
    void this.signOutService.signOut();
  }
}
