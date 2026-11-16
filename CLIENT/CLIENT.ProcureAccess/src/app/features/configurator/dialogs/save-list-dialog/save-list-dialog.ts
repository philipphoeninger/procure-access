// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  ElementRef,
  inject,
  Injector,
  signal,
  viewChild,
} from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';

import { savedListPath } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { toSaveRequest } from '../../state/export-request';
import { EXPORT_DIALOG_PANEL_CLASS } from '../export-dialog/export-dialog';

/** Longest list name (SPEC §7.2, R-SAV-01), counted like the server's `string.Length` after trimming. */
export const LIST_NAME_MAX_LENGTH = 120;

function requiredName(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

function nameLength(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > LIST_NAME_MAX_LENGTH ? { maxlength: true } : null;
}

/** A failure the dialog shows in its error notice above the actions (Handoff §5.1 S13-E). */
type SaveError = 'failed' | 'outdated' | 'notAllowed' | 'antiforgery';

/**
 * S12 · Kriterienliste speichern (R-SAV-01; UX copy §5.6; Handoff §5.3: no Figma frame, the S13
 * dialog rules): „Name der Liste“ with its hint, the note that the list is visible to every member
 * of the organisation the header shows (`criteria.save.orgNote`), „Abbrechen“ and „Liste speichern“.
 *
 * - The first focus is the name field; Esc or „Abbrechen“ close the dialog and MatDialog returns
 *   the focus to „Liste speichern“ on S11.
 * - The name is checked on submit (Handoff §6.4): required and at most 120 characters after
 *   trimming, as the server checks it; the error summary takes the focus, the field shows its error.
 * - While the list is saved the button keeps its label, shows the spinner and is `aria-disabled`.
 *   Success: S11 becomes the saved list at `/kriterienlisten/:id`, the draft is dropped (owner
 *   decision S5-20 4a) and `criteria.save.success` is announced after the route change.
 * - 403 `email-unconfirmed` (SEC-10): `pa-confirm-required-notice` shows and takes the focus; the
 *   name is kept for a second try after the confirmation, as on S20.
 * - Any other failure shows the error notice `criteria.save.error.title` (`role="alert"`):
 *   `release-mismatch` `criteria.save.outdated` with „Schließen“ only, like S13; a 403 for the role
 *   `criteria.save.notAllowed`; `antiforgery` its text; everything else `criteria.save.error`. A
 *   field error at `name` shows at the field. The request is marked `CALLER_SHOWS_FAILURES`.
 *
 * Below 600 px the dialog fills the screen and stacks its actions in DOM order (S13-M).
 *
 * @spec docs/SPEC_v2.md R-SAV-01, §8.10; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants SEC-10, API-02, A11Y-09
 */
@Component({
  selector: 'pa-save-list-dialog',
  imports: [
    ConfirmRequiredNotice,
    ErrorSummary,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './save-list-dialog.html',
  styleUrl: './save-list-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SaveListDialog {
  /** Opens S12 with the first focus in the name field. */
  static open(dialog: MatDialog): MatDialogRef<SaveListDialog> {
    return dialog.open(SaveListDialog, {
      autoFocus: 'input',
      panelClass: EXPORT_DIALOG_PANEL_CLASS,
      maxWidth: '100vw',
    });
  }

  private readonly store = inject(ProcureAccessStore);
  private readonly api = inject(ConfiguratorApiService);
  private readonly router = inject(Router);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject<MatDialogRef<SaveListDialog>>(MatDialogRef);
  private readonly injector = inject(Injector);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');
  private readonly closeButton = viewChild('closeButton', { read: ElementRef });

  /** The organisation the header shows when the dialog opens; the list is saved for it (R-SHL-03). */
  protected readonly organization = this.store.activeOrganization();
  /** `{{org}}` of `criteria.save.orgNote` and `criteria.save.notAllowed`. */
  protected readonly organizationName = this.organization?.name ?? '';

  protected readonly name = new FormControl('', { nonNullable: true, validators: [requiredName, nameLength] });
  protected readonly nameId = 'pa-save-list-name';
  protected readonly hintId = 'pa-save-list-name-hint';

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly error = signal<SaveError | null>(null);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);
  protected readonly outdated = computed(() => this.error() === 'outdated');

  private readonly submitted = signal(false);
  private destroyed = false;

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
  }

  /** The message at the name field, also the line of the error summary (E12). */
  protected nameMessage(): TextKey | null {
    if (this.name.hasError('server')) return { key: 'errors.field.server', params: { message: this.name.getError('server') } };
    if (this.name.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('criteria.save.name') as string } };
    }
    if (this.name.hasError('maxlength')) {
      return { key: 'errors.field.maxLength', params: { max: LIST_NAME_MAX_LENGTH, current: this.name.value.trim().length } };
    }
    return null;
  }

  protected errorKey(error: SaveError): string {
    switch (error) {
      case 'outdated':
        return 'criteria.save.outdated';
      case 'notAllowed':
        return 'criteria.save.notAllowed';
      case 'antiforgery':
        return 'errors.antiforgery';
      default:
        return 'criteria.save.error';
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) return;
    this.submitted.set(true);
    const message = this.nameMessage();
    if (message !== null) {
      this.summary.set([{ message, field: { id: this.nameId, labelKey: 'criteria.save.name' } }]);
      return;
    }

    const answer = this.store.configurator.list.answer();
    const selection = this.store.configurator.list.selection();
    const organization = this.organization;
    if (answer === null || selection === null || organization === null) {
      this.showError('failed');
      return;
    }

    const name = this.name.value.trim();
    this.summary.set([]);
    this.error.set(null);
    this.busy.set(true);
    let id: string;
    try {
      id = await this.api.save(
        toSaveRequest(organization.id, name, answer, selection, this.store.configurator.list.decisions()),
      );
    } catch (error: unknown) {
      if (!this.destroyed) this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }

    // S11 becomes the saved list; the draft is dropped once S11 is gone (owner decision S5-20 4a).
    this.dialogRef.close(id);
    await this.router.navigateByUrl(savedListPath(id));
    this.store.resetConfigurator();
    void this.announcer.announce('criteria.save.success', { name });
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'release-mismatch':
        this.showError('outdated');
        // „Liste speichern“ is gone: the focus moves to „Schließen“, which replaces it (as S13).
        afterNextRender(() => (this.closeButton()?.nativeElement as HTMLElement | undefined)?.focus(), {
          injector: this.injector,
        });
        return;
      case 'antiforgery':
        this.showError('antiforgery');
        return;
      case 'validation': {
        const lines = applyServerErrors(error, (path) =>
          path === 'name' ? { control: this.name, id: this.nameId, labelKey: 'criteria.save.name' } : undefined,
        );
        // Only the name is the user's to fix here; any other field error means the draft cannot be saved.
        if (this.name.hasError('server')) {
          this.summary.set(lines.filter((line) => line.field?.id === this.nameId));
        } else {
          this.showError('failed');
        }
        return;
      }
      default:
        this.showError(problemStatus(error) === 403 ? 'notAllowed' : 'failed');
    }
  }

  private showError(error: SaveError): void {
    this.error.set(error);
  }
}

/** The HTTP status of a failed request; 0 without an answer. */
function problemStatus(error: unknown): number {
  return typeof error === 'object' && error !== null && 'status' in error && typeof error.status === 'number' ? error.status : 0;
}
