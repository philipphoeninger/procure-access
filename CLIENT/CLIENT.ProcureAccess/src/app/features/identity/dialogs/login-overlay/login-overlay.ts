// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';

/** `Validators.required` as a plain function (the lint rule `unbound-method` refuses the static method). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** What the overlay shows above the fields; the failures keep the form. */
type OverlayStatus = 'ready' | 'busy' | 'invalid' | 'antiforgery';

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
 * Minimal in S5-08 (owner, sprint 05): S5-09 completes it into the S01 form card (password
 * toggle, „Passwort vergessen?“, error summary) and shares the form with the `login` outlet.
 *
 * @spec docs/SPEC_v2.md R-AUTH-07, §8.8
 * @invariants SEC-03
 */
@Component({
  selector: 'pa-login-overlay',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
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

  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly dialogRef = inject<MatDialogRef<LoginOverlay, boolean>>(MatDialogRef);

  protected readonly email = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly password = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly status = signal<OverlayStatus>('ready');
  protected readonly busy = computed(() => this.status() === 'busy');
  /** The text of the error notice; null while there is none. */
  protected readonly errorKey = computed(() => {
    switch (this.status()) {
      case 'invalid':
        return 'auth.login.error.invalid';
      case 'antiforgery':
        return 'errors.antiforgery';
      default:
        return null;
    }
  });

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.email.markAsTouched();
    this.password.markAsTouched();
    if (this.email.invalid || this.password.invalid) {
      return;
    }
    this.status.set('busy');
    try {
      await this.store.signIn(this.email.value, this.password.value);
    } catch (error) {
      this.status.set(problemTypeName(error) === 'antiforgery' ? 'antiforgery' : 'invalid');
      return;
    }
    void this.announcer.announce('shell.session.restored');
    this.dialogRef.close(true);
  }
}
