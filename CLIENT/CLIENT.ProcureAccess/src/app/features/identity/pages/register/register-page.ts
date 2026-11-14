// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
} from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { RouterLink } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemDetails, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';
import { PASSWORD_RULE } from '../../models/password-rule';
import { AuthService } from '../../services/auth.service';

/** A required text field (the lint rule `unbound-method` refuses the static `Validators.required`). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** One plain address, as Angular checks it; the server checks again (SPEC §8.8). */
function emailFormat(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? null : Validators.email(control);
}

/** The ticked privacy acknowledgement. */
function acknowledged(control: AbstractControl<boolean>): ValidationErrors | null {
  return control.value ? null : { required: true };
}

/** The visible text of the link in the consent label: the H1 of the privacy page, without its soft hyphen. */
function linkText(title: string): string {
  return title.replaceAll('­', '');
}

/** The consent label split around the link to the privacy page; `link` is empty when the label does not contain it. */
interface ConsentLabel {
  before: string;
  link: string;
  after: string;
}

type Field = ServerErrorField & { path: string; message: () => TextKey | null };

/**
 * S02 „Konto erstellen“ at `/registrieren` (SPEC R-AUTH-01, R-AUTH-02; UX copy §5.2; Handoff §5.3:
 * no Figma frame, form card as in S51 and S01).
 *
 * H1, intro, the form card with `common.requiredNote`, „E-Mail-Adresse“, „Passwort“ with the hint
 * of the 🔧 minimum and „Passwort anzeigen“, the privacy note and the required acknowledgement,
 * whose link „Datenschutzhinweise“ opens the privacy page in a new tab so the form is kept; then
 * „Konto erstellen“ with the button spinner of Handoff §6.2.
 *
 * Fields are checked on submit (Handoff §6.4): required fields, the address format, the length of
 * the password (`errors.field.minLength` / `.maxLength` with the current count) and the
 * acknowledgement; the server checks the same and decides. A failed submit shows the error summary,
 * which takes the focus, and „Fehler:“ in the page title; the server's field errors go to their
 * fields, a missing antiforgery token and the rate limit into the summary. The server answers a new
 * and a known address alike (no account enumeration), so success always replaces the form with
 * `auth.register.success.title` (focused) and the text naming the address.
 *
 * @spec docs/SPEC_v2.md R-AUTH-01, R-AUTH-02; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants API-02, A11Y-09
 */
@Component({
  selector: 'pa-register-page',
  imports: [
    ErrorSummary,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    ReactiveFormsModule,
    RouterLink,
    TranslatePipe,
  ],
  templateUrl: './register-page.html',
  styleUrl: './register-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterPage {
  private readonly authService = inject(AuthService);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly injector = inject(Injector);

  protected readonly rule = inject(PASSWORD_RULE);
  protected readonly email = new FormControl('', { nonNullable: true, validators: [required, emailFormat] });
  protected readonly password = new FormControl('', {
    nonNullable: true,
    validators: [required, Validators.minLength(this.rule.minLength), Validators.maxLength(this.rule.maxLength)],
  });
  protected readonly consent = new FormControl(false, { nonNullable: true, validators: [acknowledged] });

  protected readonly busy = signal(false);
  protected readonly passwordShown = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  /** The address the success text names; null while the form shows. */
  protected readonly registeredEmail = signal<string | null>(null);

  protected readonly emailId = 'pa-register-email';
  protected readonly passwordId = 'pa-register-password';
  protected readonly consentId = 'pa-register-consent';
  /** MatCheckbox gives its input the host id plus `-input`; the summary links there. */
  protected readonly consentInputId = `${this.consentId}-input`;
  protected readonly consentErrorId = `${this.consentId}-error`;

  private readonly submitted = signal(false);
  private readonly successHeading = viewChild<ElementRef<HTMLElement>>('successHeading');

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  /** The consent error shows under the checkbox once a submit found it missing or the server refused it. */
  protected readonly consentError = signal<TextKey | null>(null);

  /**
   * The consent label around its link „Datenschutzhinweise“ (UX copy §5.2): the link text is the
   * H1 of the privacy page, found in the label, so no German text is in code. Read on every render,
   * so it follows the texts once they are loaded.
   */
  protected consentLabel(): ConsentLabel {
    const label = this.translate.instant('auth.register.consent') as string;
    const link = linkText(this.translate.instant('legal.privacy.title') as string);
    const at = link.length > 0 ? label.indexOf(link) : -1;
    return at < 0 ? { before: label, link: '', after: '' } : { before: label.slice(0, at), link, after: label.slice(at + link.length) };
  }

  /** Ticking the box clears its error; unticking it after a submit shows it again. */
  protected consentChanged(): void {
    this.consentError.set(this.submitted() && this.consent.hasError('required') ? { key: 'auth.register.consent.required' } : null);
  }

  private readonly fields: Field[] = [
    {
      path: 'email',
      control: this.email,
      id: this.emailId,
      labelKey: 'auth.register.email',
      message: () => this.emailMessage(),
    },
    {
      path: 'password',
      control: this.password,
      id: this.passwordId,
      labelKey: 'auth.register.password',
      message: () => this.passwordMessage(),
    },
    {
      path: 'privacyAcknowledged',
      control: this.consent,
      id: this.consentInputId,
      labelKey: 'auth.register.consent',
      message: () => (this.consent.hasError('required') ? { key: 'auth.register.consent.required' } : null),
    },
  ];

  protected togglePassword(): void {
    this.passwordShown.update((shown) => !shown);
  }

  /** The message at the e-mail field, also the line of the error summary (E12). */
  protected emailMessage(): TextKey | null {
    if (this.email.hasError('server')) return { key: 'errors.field.server', params: { message: this.email.getError('server') } };
    if (this.email.hasError('required')) return this.requiredMessage('auth.register.email');
    if (this.email.hasError('email')) return { key: 'errors.field.email' };
    return null;
  }

  /** The message at the password field, also the line of the error summary (E12). */
  protected passwordMessage(): TextKey | null {
    const current = this.password.value.length;
    if (this.password.hasError('server')) return { key: 'errors.field.server', params: { message: this.password.getError('server') } };
    if (this.password.hasError('required')) return this.requiredMessage('auth.register.password');
    if (this.password.hasError('minlength')) return { key: 'errors.field.minLength', params: { min: this.rule.minLength, current } };
    if (this.password.hasError('maxlength')) return { key: 'errors.field.maxLength', params: { max: this.rule.maxLength, current } };
    return null;
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    this.consentError.set(this.consent.hasError('required') ? { key: 'auth.register.consent.required' } : null);
    const invalid = this.fieldLines();
    if (invalid.length > 0) {
      this.showErrors(invalid);
      return;
    }
    const email = this.email.value.trim();
    this.busy.set(true);
    try {
      await this.authService.register({ email, password: this.password.value, privacyAcknowledged: true });
    } catch (error: unknown) {
      this.showErrors(this.serverErrors(error));
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    this.registeredEmail.set(email);
    afterNextRender(() => this.successHeading()?.nativeElement.focus(), { injector: this.injector });
  }

  private requiredMessage(labelKey: string): TextKey {
    return { key: 'errors.field.required', params: { field: this.translate.instant(labelKey) as string } };
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.pageTitle.setPageTitleState({ error: lines.length > 0 });
  }

  /** One linked line per field with an error, with the text the field shows. */
  private fieldLines(): ErrorSummaryItem[] {
    return this.fields.flatMap((field) => {
      const message = field.message();
      return message ? [{ message, field: { id: field.id, labelKey: field.labelKey } }] : [];
    });
  }

  /**
   * The lines of a refused registration; none for a failure the error interceptor shows itself.
   * The server's field errors go to their fields (API-02).
   */
  private serverErrors(error: unknown): ErrorSummaryItem[] {
    switch (problemTypeName(error)) {
      case 'antiforgery':
        return [{ message: { key: 'errors.antiforgery' } }];
      case 'rate-limit': {
        const minutes = problemDetails(error)?.retryAfterMinutes ?? 1;
        return [{ message: { key: pluralKey(this.translate, 'errors.rateLimit', minutes), params: { minutes } } }];
      }
      case 'validation': {
        const lines = applyServerErrors(error, (path) => this.fields.find((field) => field.path === path));
        if (this.consent.hasError('server')) {
          this.consentError.set({ key: 'errors.field.server', params: { message: this.consent.getError('server') as string } });
        }
        return lines;
      }
      default:
        return [];
    }
  }
}
