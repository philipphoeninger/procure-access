// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemFieldErrors, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { LOCKOUT_RULE } from '../../models/lockout-rule';
import { RESET_PASSWORD_PATH } from '../../models/login-navigation';

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
 * a locked or a deleted account, so it never names a field (SPEC §8.8) — followed by the fixed
 * lockout note `auth.login.error.lockout` with the 🔧 numbers (owner decisions S5-13 1 (a),
 * S5-13b 2 (a): the server never says whether an account is locked) — or `errors.antiforgery`.
 * Other failures keep the system states of the error interceptor. A successful sign-in emits
 * `signedIn`; the host decides what follows.
 *
 * „Passwort vergessen?“ is a link to S03 (S5-13b): on S01 in place of the page, closing the `login`
 * outlet; in the login overlay (`forgotInNewTab`) in a new tab, so the page under the overlay keeps
 * its input (owner decision S5-13b 1 (a)).
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
    RouterLink,
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
  /** „Passwort vergessen?“ opens S03 in a new tab (the login overlay) instead of in place (S01). */
  readonly forgotInNewTab = input(false);
  /** The user is signed in. */
  readonly signedIn = output();
  /** True while the error summary shows lines, for the page title prefix „Fehler:“ (UX copy §6.1). */
  readonly errorsShown = output<boolean>();

  private readonly store = inject(ProcureAccessStore);
  private readonly translate = inject(TranslateService);
  private readonly lockout = inject(LOCKOUT_RULE);

  protected readonly email = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly password = new FormControl('', { nonNullable: true, validators: [required] });
  protected readonly busy = signal(false);
  protected readonly passwordShown = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  /** The lockout note under `auth.login.error.invalid`; null for every other summary. */
  protected readonly summaryNote = signal<TextKey | null>(null);
  protected readonly resetPath = RESET_PASSWORD_PATH;
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
      const lines = this.serverErrors(error);
      this.showErrors(lines, this.refusedAsAWhole(error) ? this.lockoutNote() : null);
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    this.signedIn.emit();
  }

  private showErrors(lines: ErrorSummaryItem[], note: TextKey | null = null): void {
    this.summaryNote.set(note);
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
        return this.refusedAsAWhole(error)
          ? [{ message: { key: 'auth.login.error.invalid' } }]
          : applyServerErrors(error, (path) => this.fields().find((field) => field.path === path));
      default:
        return [];
    }
  }

  /** The server refused the sign-in itself (an error at `$`), not a field. */
  private refusedAsAWhole(error: unknown): boolean {
    return problemTypeName(error) === 'validation' && BODY_PATH in problemFieldErrors(error);
  }

  private lockoutNote(): TextKey {
    return { key: 'auth.login.error.lockout', params: { attempts: this.lockout.maxFailedAttempts, minutes: this.lockout.minutes } };
  }

  private fields(): (ServerErrorField & { path: string })[] {
    return [
      { path: 'email', control: this.email, id: this.emailId(), labelKey: 'auth.login.email' },
      { path: 'password', control: this.password, id: this.passwordId(), labelKey: 'auth.login.password' },
    ];
  }
}
