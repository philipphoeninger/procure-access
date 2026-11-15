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
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemTypeName, ServerErrorField } from '@app/core/errors/problem-details';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';
import { MEMBERS_SEGMENT, ORGANIZATION_PATH, ORGANIZATION_TYPES, OrganizationType, typeKey } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';

/** Longest organisation name (SPEC §7.2), counted like the server's `string.Length`. */
export const ORGANIZATION_NAME_MAX_LENGTH = 120;

function requiredName(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

function nameLength(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > ORGANIZATION_NAME_MAX_LENGTH ? { maxlength: true } : null;
}

function requiredType(control: AbstractControl<OrganizationType | null>): ValidationErrors | null {
  return control.value === null ? { required: true } : null;
}

type Field = ServerErrorField & { path: string; message: () => TextKey | null };

/**
 * S20 „Organisation anlegen oder beitreten“ at `/organisation` (SPEC R-ORG-01, R-ORG-06; UX copy
 * §5.7; Handoff §5.3: no Figma frame, form card as in S51 and S02).
 *
 * H1, intro, the note that contributing needs no organisation, the form card with
 * `common.requiredNote`, „Name der Organisation“ and the radio group „Art der Organisation“, then
 * „Organisation anlegen“ with the button spinner, and after the form how to join an existing
 * organisation (by invitation only, R-ORG-06). Fields are checked on submit (Handoff §6.4): the
 * name is required and at most 120 characters after trimming, the type required (`errors.field.requiredChoice`); the server checks
 * the same. A failed submit shows the error summary with the focus. The server refuses an
 * unconfirmed address with 403 `email-unconfirmed` (SEC-10): then `pa-confirm-required-notice`
 * shows above the form and takes the focus, and the input is kept for a second submit after the
 * confirmation. Success opens S21 of the new organisation, whose route change titles, focuses and
 * announces it.
 *
 * @spec docs/SPEC_v2.md R-ORG-01, R-ORG-06, §8.9; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants API-02, SEC-10, A11Y-09
 */
@Component({
  selector: 'pa-organization-start-page',
  imports: [
    ConfirmRequiredNotice,
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
  templateUrl: './organization-start-page.html',
  styleUrl: './organization-start-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class OrganizationStartPage {
  private readonly api = inject(OrganizationApiService);
  private readonly router = inject(Router);
  private readonly translate = inject(TranslateService);
  private readonly pageTitle = inject(PageTitleStrategy);
  private readonly injector = inject(Injector);

  protected readonly types = ORGANIZATION_TYPES;
  protected readonly typeKey = typeKey;
  protected readonly name = new FormControl('', { nonNullable: true, validators: [requiredName, nameLength] });
  protected readonly type = new FormControl<OrganizationType | null>(null, { validators: [requiredType] });

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);
  protected readonly typeError = signal<TextKey | null>(null);

  protected readonly nameId = 'pa-org-name';
  protected readonly typeGroupId = 'pa-org-type';
  protected readonly typeErrorId = 'pa-org-type-error';
  /** The summary links to the first option; MatRadioButton gives its input the host id plus `-input`. */
  protected readonly firstTypeInputId = `pa-org-type-${ORGANIZATION_TYPES[0]}-input`;

  private readonly submitted = signal(false);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  private readonly fields: Field[] = [
    { path: 'name', control: this.name, id: this.nameId, labelKey: 'org.create.name', message: () => this.nameMessage() },
    {
      path: 'type',
      control: this.type,
      id: this.firstTypeInputId,
      labelKey: 'org.create.type',
      message: () => this.typeMessage(),
    },
  ];

  /** The message at the name field, also the line of the error summary (E12). */
  protected nameMessage(): TextKey | null {
    if (this.name.hasError('server')) return { key: 'errors.field.server', params: { message: this.name.getError('server') } };
    if (this.name.hasError('required')) return this.requiredMessage('org.create.name');
    if (this.name.hasError('maxlength')) {
      return { key: 'errors.field.maxLength', params: { max: ORGANIZATION_NAME_MAX_LENGTH, current: this.name.value.trim().length } };
    }
    return null;
  }

  /** The message under the type group, also the line of the error summary. */
  protected typeMessage(): TextKey | null {
    if (this.type.hasError('server')) return { key: 'errors.field.server', params: { message: this.type.getError('server') } };
    if (this.type.hasError('required')) return { key: 'errors.field.requiredChoice' };
    return null;
  }

  /** Choosing a type clears its error. */
  protected typeChanged(): void {
    this.typeError.set(this.submitted() ? this.typeMessage() : null);
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) {
      return;
    }
    this.submitted.set(true);
    this.typeError.set(this.typeMessage());
    const invalid = this.fieldLines();
    if (invalid.length > 0) {
      this.showErrors(invalid);
      return;
    }
    this.busy.set(true);
    let id: string;
    try {
      ({ id } = await this.api.create({ name: this.name.value.trim(), type: this.type.value! }));
    } catch (error: unknown) {
      this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }
    this.showErrors([]);
    await this.router.navigate([ORGANIZATION_PATH, id, MEMBERS_SEGMENT]);
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.showErrors([]);
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'antiforgery':
        this.showErrors([{ message: { key: 'errors.antiforgery' } }]);
        return;
      case 'validation': {
        const lines = applyServerErrors(error, (path) => this.fields.find((field) => field.path === path));
        this.typeError.set(this.typeMessage());
        this.showErrors(lines);
        return;
      }
      default:
        // Anything else keeps the system states of the error interceptor.
        this.showErrors([]);
    }
  }

  private requiredMessage(labelKey: string): TextKey {
    return { key: 'errors.field.required', params: { field: this.translate.instant(labelKey) as string } };
  }

  private showErrors(lines: ErrorSummaryItem[]): void {
    this.summary.set(lines);
    this.pageTitle.setPageTitleState({ error: lines.length > 0 });
  }

  private fieldLines(): ErrorSummaryItem[] {
    return this.fields.flatMap((field) => {
      const message = field.message();
      return message ? [{ message, field: { id: field.id, labelKey: field.labelKey } }] : [];
    });
  }
}
