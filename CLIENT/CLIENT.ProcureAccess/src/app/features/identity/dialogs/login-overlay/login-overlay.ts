// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, DestroyRef, inject, Injector } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { LoginForm } from '../../components/login-form/login-form';
import { AuthService } from '../../services/auth.service';
import { SignOutService } from '../../services/sign-out.service';

/**
 * Who was signed in before the session ended. Left out after a 401, where the store still holds
 * that user; the session warning passes it, because its probe has already cleared the session.
 */
export interface LoginOverlayData {
  previousUserId?: string | null;
}

/** Panel class of the dialog: 600 px from 600 px, full screen below (`styles/main.scss`, Handoff §2.3). */
export const LOGIN_OVERLAY_PANEL_CLASS = 'pa-dialog';

/**
 * The login overlay after a 401 (ARCHITECTURE §8.3, C60; SPEC R-AUTH-07; Handoff §5.3 „Login
 * overlay“): `shell.session.expired.title` and `.body`, the sign-in fields of S01 and „Anmelden“.
 * Opened by `LoginOverlayService` from the session interceptor, never by navigation, so the page
 * and its input stay underneath, and by the session warning when the limit passed (owner decision
 * S5-30 D1 (b)). A successful sign-in of the same account announces `shell.session.restored` and
 * closes with `true`, so the interceptor retries the held request. Another account (owner decision
 * S5-30 D3 (b)) gets nothing of the previous user's: the overlay closes with `false` (no retry),
 * their in-memory data goes as after a sign-out and the start page shows with the focus on its H1.
 * „Schließen“ and Esc close with nothing; the session is then read again, so the header shows the
 * user signed out on the same page (R-AUTH-07).
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
  /**
   * Calls the probe first, then opens the overlay with the first focus in the e-mail field. The
   * probe issues the antiforgery token of the anonymous state: the old token belongs to the ended
   * session, and the sign-in would be refused with it (S5-30). Its answer is not applied, so the
   * page underneath keeps the signed-in state and its input until the user signs in or closes the
   * overlay; a failed probe opens the overlay all the same.
   */
  static async open(
    dialog: MatDialog,
    injector: Injector,
    data: LoginOverlayData = {},
  ): Promise<MatDialogRef<LoginOverlay, boolean>> {
    const previousUserId =
      data.previousUserId !== undefined ? data.previousUserId : (injector.get(ProcureAccessStore).session()?.id ?? null);
    await injector.get(AuthService).session().catch(() => undefined);
    return dialog.open<LoginOverlay, LoginOverlayData, boolean>(LoginOverlay, {
      data: { previousUserId },
      autoFocus: 'input[name="email"]',
      panelClass: LOGIN_OVERLAY_PANEL_CLASS,
      maxWidth: '100vw',
    });
  }

  private readonly announcer = inject(A11yAnnouncerService);
  private readonly dialogRef = inject<MatDialogRef<LoginOverlay, boolean>>(MatDialogRef);
  private readonly store = inject(ProcureAccessStore);
  private readonly signOutService = inject(SignOutService);
  private readonly data = inject<LoginOverlayData | null>(MAT_DIALOG_DATA, { optional: true });
  private readonly previousUserId =
    this.data?.previousUserId !== undefined ? this.data.previousUserId : (this.store.session()?.id ?? null);
  private signedInHere = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => {
      if (!this.signedInHere) {
        void this.store.refreshSession();
      }
    });
  }

  protected signedIn(): void {
    this.signedInHere = true;
    const userId = this.store.session()?.id ?? null;
    if (this.previousUserId !== null && userId !== this.previousUserId) {
      this.dialogRef.close(false);
      void this.signOutService.anotherUserSignedIn();
      return;
    }
    void this.announcer.announce('shell.session.restored');
    this.dialogRef.close(true);
  }
}
