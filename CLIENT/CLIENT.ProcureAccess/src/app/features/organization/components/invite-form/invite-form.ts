// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject, input, output, signal } from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemDetails, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';
import { pluralKey } from '@app/shared/pipes/translate-plural.pipe';
import { ORGANIZATION_ROLES, OrganizationRole, roleKey } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';

/** A required text field (the lint rule `unbound-method` refuses the static `Validators.required`). */
function required(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

/** One plain address, as Angular checks it; the server checks again (SPEC §8.9). */
function emailFormat(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? null : Validators.email(control);
}

function requiredRole(control: AbstractControl<OrganizationRole | null>): ValidationErrors | null {
  return control.value === null ? { required: true } : null;
}

type Field = ServerErrorField & { path: string; message: () => TextKey | null };

/**
 * „Mitglied einladen“ on S21, for Owners (SPEC R-ORG-02, §8.9; UX copy U136; Handoff §5.3 form
 * card, §6.4 errors): the address and the role as three radio buttons with their hints, no
 * preselection (owner decision S5-17b 1 (a)), then „Einladung senden“ with the button spinner.
 *
 * Fields are checked on submit: the address required and valid, and not the address of a member —
 * checked here against {@link memberEmails}, because the server refuses it only with its own text —,
 * the role required (`errors.field.requiredChoice`). A failed submit shows the error summary with
 * the focus. Success announces `org.invite.sent` with the 🔧 lifetime of the answer, clears the form
 * and emits {@link invited}. A failed send (503 `email-delivery`, the invitation is kept), the rate
 * limit (429) and a missing antiforgery token show the error notice `org.invite.error.title` above
 * the button; after a failed send {@link invited} is emitted too, so the list shows the kept
 * invitation. Inviting again replaces it (owner decision S5-17 D3 (a)).
 *
 * @spec docs/SPEC_v2.md R-ORG-02, §8.9; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants API-02, A11Y-09
 */
@Component({
  selector: 'pa-invite-form',
  imports: [
    ErrorSummary,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './invite-form.html',
  styleUrl: './invite-form.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class InviteForm {
  private readonly api = inject(OrganizationApiService);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);

  /** The organisation that invites. */
  readonly organizationId = input.required<string>();
  /** The members' addresses, for `org.invite.alreadyMember` before anything is sent. */
  readonly memberEmails = input.required<(string | null)[]>();
  /** An invitation was stored: after a sent e-mail and after a failed send, which keeps it. */
  readonly invited = output<void>();
  /** True while the error summary shows field errors, for the page title's „Fehler:“. */
  readonly errorsShown = output<boolean>();

  protected readonly roles = ORGANIZATION_ROLES;
  protected readonly roleKey = roleKey;
  protected readonly email = new FormControl('', {
    nonNullable: true,
    validators: [required, emailFormat, (control: AbstractControl<string>) => this.notAMember(control)],
  });
  protected readonly role = new FormControl<OrganizationRole | null>(null, { validators: [requiredRole] });

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly roleError = signal<TextKey | null>(null);
  /** The text of the error notice above the button; null while there is none. */
  protected readonly failure = signal<TextKey | null>(null);

  protected readonly headingId = 'pa-invite-title';
  protected readonly emailId = 'pa-invite-email';
  protected readonly roleErrorId = 'pa-invite-role-error';
  /** The summary links to the first option; MatRadioButton gives its input the host id plus `-input`. */
  protected readonly firstRoleInputId = `pa-invite-role-${ORGANIZATION_ROLES[0]}-input`;

  private readonly submitted = signal(false);

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  private readonly fields: Field[] = [
    { path: 'email', control: this.email, id: this.emailId, labelKey: 'org.invite.email', message: () => this.emailMessage() },
    { path: 'role', control: this.role, id: this.firstRoleInputId, labelKey: 'org.invite.role', message: () => this.roleMessage() },
  ];

  /** The message at the address field, also the line of the error summary. */
  protected emailMessage(): TextKey | null {
    if (this.email.hasError('server')) return { key: 'errors.field.server', params: { message: this.email.getError('server') } };
    if (this.email.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('org.invite.email') as string } };
    }
    if (this.email.hasError('email')) return { key: 'errors.field.email' };
    if (this.email.hasError('member')) return { key: 'org.invite.alreadyMember' };
    return null;
  }

  /** The message under the role group, also the line of the error summary. */
  protected roleMessage(): TextKey | null {
    if (this.role.hasError('server')) return { key: 'errors.field.server', params: { message: this.role.getError('server') } };
    if (this.role.hasError('required')) return { key: 'errors.field.requiredChoice' };
    return null;
  }

  /** Choosing a role clears its error. */
  protected roleChanged(): void {
    this.roleError.set(this.submitted() ? this.roleMessage() : null);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    this.failure.set(null);
    this.email.updateValueAndValidity();
    this.roleError.set(this.roleMessage());
    const invalid = this.fieldLines();
    if (invalid.length > 0) {
      this.showErrors(invalid);
      return;
    }
    const email = this.email.value.trim();
    this.busy.set(true);
    let validDays: number;
    try {
      ({ validDays } = await this.api.invite(this.organizationId(), { email, role: this.role.value! }));
    } catch (error: unknown) {
      this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    void this.announcer.announce('org.invite.sent', { email, days: validDays });
    this.submitted.set(false);
    this.email.reset();
    this.role.reset();
    this.invited.emit();
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'validation': {
        const lines = applyServerErrors(error, (path) => this.fields.find((field) => field.path === path));
        this.roleError.set(this.roleMessage());
        this.showErrors(lines);
        return;
      }
      case 'email-delivery':
        this.showErrors([]);
        this.failure.set({ key: 'errors.email.sendFailed' });
        // The server keeps the invitation (SPEC §7.6): the list shows it.
        this.invited.emit();
        return;
      case 'rate-limit': {
        this.showErrors([]);
        const minutes = problemDetails(error)?.retryAfterMinutes ?? 1;
        this.failure.set({ key: pluralKey(this.translate, 'errors.rateLimit', minutes), params: { minutes } });
        return;
      }
      case 'antiforgery':
        this.showErrors([]);
        this.failure.set({ key: 'errors.antiforgery' });
        return;
      default:
        // Anything else keeps the system states of the error interceptor.
        this.showErrors([]);
    }
  }

  private notAMember(control: AbstractControl<string>): ValidationErrors | null {
    const address = control.value.trim().toLowerCase();
    return address.length > 0 && this.memberEmails().some((member) => member?.toLowerCase() === address) ? { member: true } : null;
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.errorsShown.emit(lines.length > 0);
  }

  private fieldLines(): ErrorSummaryItem[] {
    return this.fields.flatMap((field) => {
      const message = field.message();
      return message ? [{ message, field: { id: field.id, labelKey: field.labelKey } }] : [];
    });
  }
}
