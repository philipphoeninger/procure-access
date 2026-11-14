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
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemDetails, problemTypeName } from '@app/core/errors/problem-details';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';
import { AuthService } from '../../services/auth.service';

/** A required text field (the lint rule `unbound-method` refuses the static `Validators.required`). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** One plain address, as Angular checks it; the server checks again (SPEC §8.8). */
function emailFormat(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? null : Validators.email(control);
}

/** What the confirmation names: the address as sent and the link's lifetime from the answer. */
interface SentTo {
  email: string;
  hours: number;
}

/**
 * S03 „Passwort zurücksetzen“, the request step at `/passwort` (SPEC R-AUTH-04; UX copy §5.2;
 * Handoff §5.3: no Figma frame, form card as in S51, S01 and S02).
 *
 * H1, intro, the form card with `common.requiredNote` and „E-Mail-Adresse“ (`auth.login.email`, the
 * same label as S01), then „Link anfordern“ with the button spinner of Handoff §6.2. The address is
 * checked on submit (required, format; Handoff §6.4); a failed submit shows the error summary with
 * the focus and „Fehler:“ in the page title, the server's field error goes to the field, a missing
 * antiforgery token and the rate limit into the summary. The server answers every address alike
 * (no account enumeration) with the link's 🔧 lifetime, so success always replaces the form with
 * `auth.reset.sent`, which takes the focus.
 *
 * @spec docs/SPEC_v2.md R-AUTH-04; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants API-02, A11Y-09
 */
@Component({
  selector: 'pa-reset-request-page',
  imports: [ErrorSummary, MatButtonModule, MatFormFieldModule, MatInputModule, MatProgressSpinnerModule, ReactiveFormsModule, TranslatePipe],
  templateUrl: './reset-request-page.html',
  styleUrl: '../reset-password-pages.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetRequestPage {
  private readonly authService = inject(AuthService);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly injector = inject(Injector);

  protected readonly email = new FormControl('', { nonNullable: true, validators: [required, emailFormat] });
  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly sentTo = signal<SentTo | null>(null);
  protected readonly emailId = 'pa-reset-email';

  private readonly submitted = signal(false);
  private readonly sentText = viewChild<ElementRef<HTMLElement>>('sentText');

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  /** The message at the field, also the line of the error summary (E12). */
  protected emailMessage(): TextKey | null {
    if (this.email.hasError('server')) return { key: 'errors.field.server', params: { message: this.email.getError('server') } };
    if (this.email.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('auth.login.email') as string } };
    }
    if (this.email.hasError('email')) return { key: 'errors.field.email' };
    return null;
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    const message = this.emailMessage();
    if (message) {
      this.showErrors([{ message, field: { id: this.emailId, labelKey: 'auth.login.email' } }]);
      return;
    }
    const email = this.email.value.trim();
    this.busy.set(true);
    let hours: number;
    try {
      ({ validHours: hours } = await this.authService.requestPasswordReset(email));
    } catch (error: unknown) {
      this.showErrors(this.serverErrors(error));
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    this.sentTo.set({ email, hours });
    afterNextRender(() => this.sentText()?.nativeElement.focus(), { injector: this.injector });
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.pageTitle.setPageTitleState({ error: lines.length > 0 });
  }

  /** The lines of a refused request; none for a failure the error interceptor shows itself. */
  private serverErrors(error: unknown): ErrorSummaryItem[] {
    switch (problemTypeName(error)) {
      case 'antiforgery':
        return [{ message: { key: 'errors.antiforgery' } }];
      case 'rate-limit': {
        const minutes = problemDetails(error)?.retryAfterMinutes ?? 1;
        return [{ message: { key: pluralKey(this.translate, 'errors.rateLimit', minutes), params: { minutes } } }];
      }
      case 'validation':
        return applyServerErrors(error, (path) =>
          path === 'email' ? { control: this.email, id: this.emailId, labelKey: 'auth.login.email' } : undefined,
        );
      default:
        return [];
    }
  }
}
