// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs';

import { ErrorSummaryItem } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemFieldErrors, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { ForgotPasswordDialog } from '../../dialogs/forgot-password-dialog/forgot-password-dialog';
import { AuthService } from '../../services/auth.service';

/** `Validators.required` as a plain function (the lint rule `unbound-method` refuses the static method). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** The path of `POST /auth/login` errors that concern the body as a whole (SPEC §8.8). */
const BODY_PATH = '$';

/**
 * The sign-in form of S01 and of the login overlay (SPEC R-AUTH-03, R-AUTH-07; Handoff §5.3 „Form
 * card as in S51“, §6.4): `common.requiredNote`, the outlined fields „E-Mail-Adresse“ and
 * „Passwort“ with „Passwort anzeigen“ (`aria-pressed`), „Passwort vergessen?“ and „Anmelden“ with
 * the button spinner of Handoff §6.2. A secondary action (the overlay's „Schließen“) is projected
 * before „Anmelden“ with the attribute `paLoginSecondaryAction`.
 *
 * Fields are checked on submit only. A failed submit shows the error summary at the top of the
 * form, which takes the focus: one linked line per empty field, or `auth.login.error.invalid`
 * without a field for the server's answer on the body as a whole — a wrong address or password,
 * a locked or a deleted account, so it never names a field (SPEC §8.8) — or `errors.antiforgery`.
 * Other failures keep the system states of the error interceptor. A successful sign-in emits
 * `signedIn`; the host decides what follows.
 *
 * „Passwort vergessen?“ opens the legacy reset dialog until S03 replaces it (S5-13, owner decision
 * S5-09 (a)), so it is a button, not a link.
 *
 * @spec docs/SPEC_v2.md R-AUTH-03, R-AUTH-07; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants SEC-03, API-02
 */
@Component({
  selector: 'pa-login-form',
  imports: [
    ErrorSummary,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './login-form.html',
  styleUrl: './login-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginForm {
  /** Prefix of the element ids, so S01 and the overlay never share one. */
  readonly idPrefix = input.required<string>();
  /** `card`: the fields in an outlined card, actions below it (S01); `dialog`: no card (overlay). */
  readonly formLayout = input<'card' | 'dialog'>('card');
  /** The user is signed in. */
  readonly signedIn = output();
  /** True while the error summary shows lines, for the page title prefix „Fehler:“ (UX copy §6.1). */
  readonly errorsShown = output<boolean>();

  private readonly store = inject(ProcureAccessStore);
  private readonly authService = inject(AuthService);
  private readonly dialog = inject(MatDialog);
  private readonly translate = inject(TranslateService);

  protected readonly email = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly password = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly busy = signal(false);
  protected readonly passwordShown = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly formId = computed(() => `${this.idPrefix()}-form`);
  protected readonly emailId = computed(() => `${this.idPrefix()}-email`);
  protected readonly passwordId = computed(() => `${this.idPrefix()}-password`);

  private readonly submitted = signal(false);
  /** Field errors show after the first submit (Handoff §6.4: validation on submit only), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  protected togglePassword(): void {
    this.passwordShown.update((shown) => !shown);
  }

  /** The legacy dialog asks for the address and requests the reset link (until S03, S5-13). */
  protected openForgotPassword(): void {
    const dialogRef = this.dialog.open<ForgotPasswordDialog, { email: string }, string | undefined>(ForgotPasswordDialog, {
      data: { email: this.email.value },
      width: '420px',
      maxWidth: '100vw',
    });
    dialogRef.afterClosed().subscribe((email) => {
      if (email !== undefined) {
        this.store.incrementLoadingCount();
        this.authService
          .forgotPassword(email)
          .pipe(finalize(() => this.store.decrementLoadingCount()))
          .subscribe();
      }
    });
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    const missing = this.missingFields();
    if (missing.length > 0) {
      this.showErrors(missing);
      return;
    }
    this.busy.set(true);
    try {
      await this.store.startSession(this.email.value, this.password.value);
    } catch (error: unknown) {
      this.showErrors(this.serverErrors(error));
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    this.signedIn.emit();
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.errorsShown.emit(lines.length > 0);
  }

  private missingFields(): ErrorSummaryItem[] {
    return this.fields()
      .filter((field) => field.control.hasError('required'))
      .map((field) => ({
        message: {
          key: 'errors.field.required',
          params: { field: this.translate.instant(field.labelKey) as string },
        },
        field: { id: field.id, labelKey: field.labelKey },
      }));
  }

  /** The lines of a refused sign-in; none for a failure the error interceptor shows itself. */
  private serverErrors(error: unknown): ErrorSummaryItem[] {
    switch (problemTypeName(error)) {
      case 'antiforgery':
        return [{ message: { key: 'errors.antiforgery' } }];
      case 'validation':
        return BODY_PATH in problemFieldErrors(error)
          ? [{ message: { key: 'auth.login.error.invalid' } }]
          : applyServerErrors(error, (path) => this.fields().find((field) => field.path === path));
      default:
        return [];
    }
  }

  private fields(): (ServerErrorField & { path: string })[] {
    return [
      { path: 'email', control: this.email, id: this.emailId(), labelKey: 'auth.login.email' },
      { path: 'password', control: this.password, id: this.passwordId(), labelKey: 'auth.login.password' },
    ];
  }
}
