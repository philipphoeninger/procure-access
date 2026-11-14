// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { LoginForm } from '../../components/login-form/login-form';

/** Panel class of the dialog: 600 px from 600 px, full screen below (`styles/main.scss`, Handoff §2.3). */
export const LOGIN_OVERLAY_PANEL_CLASS = 'pa-dialog';

/**
 * The login overlay after a 401 (ARCHITECTURE §8.3, C60; SPEC R-AUTH-07; Handoff §5.3 „Login
 * overlay“): `shell.session.expired.title` and `.body`, the sign-in fields of S01 and „Anmelden“.
 * Opened by `LoginOverlayService` from the session interceptor, never by navigation, so the page
 * and its input stay underneath. A successful sign-in announces `shell.session.restored` and
 * closes with `true`, so the interceptor retries the held request; „Schließen“ and Esc close with
 * nothing, and the user stays signed out on the page.
 *
 * The form is the one of S01 (`pa-login-form`, S5-09): error summary, „Passwort anzeigen“ and
 * „Passwort vergessen?“, with „Schließen“ before „Anmelden“.
 *
 * @spec docs/SPEC_v2.md R-AUTH-07, §8.8; docs/design_handoff_procureaccess_v1.md §5.3
 * @invariants SEC-03
 */
@Component({
  selector: 'pa-login-overlay',
  imports: [LoginForm, MatButtonModule, MatDialogModule, TranslatePipe],
  templateUrl: './login-overlay.html',
  styleUrl: './login-overlay.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginOverlay {
  /** Opens the overlay with the first focus in the e-mail field. */
  static open(dialog: MatDialog): MatDialogRef<LoginOverlay, boolean> {
    return dialog.open<LoginOverlay, void, boolean>(LoginOverlay, {
      autoFocus: 'input[name="email"]',
      panelClass: LOGIN_OVERLAY_PANEL_CLASS,
      maxWidth: '100vw',
    });
  }

  private readonly announcer = inject(A11yAnnouncerService);
  private readonly dialogRef = inject<MatDialogRef<LoginOverlay, boolean>>(MatDialogRef);

  protected signedIn(): void {
    void this.announcer.announce('shell.session.restored');
    this.dialogRef.close(true);
  }
}
