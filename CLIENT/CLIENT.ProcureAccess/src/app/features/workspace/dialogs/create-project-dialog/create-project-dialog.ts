// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, Injector, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemTypeName } from '@app/core/errors/problem-details';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SavedListSummary } from '@app/features/configurator/models/saved-lists';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';

import { PROJECT_NAME_MAX_LENGTH, projectPath } from '../../models/projects';

function requiredName(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

function nameLength(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > PROJECT_NAME_MAX_LENGTH ? { maxlength: true } : null;
}

/** A failure the dialog shows in its error notice above the actions (Handoff §5.1 S13-E). */
type CreateError = 'failed' | 'notAllowed' | 'antiforgery';

/** The general notice of the error interceptor, for a list that was not copied (owner decision S6-12 D5 (b)). */
const GENERAL_ERROR_KEY = 'ERROR.GENERIC';

/** The value of „Keine Liste“; the lists have UUIDs. */
const NO_LIST = '';

/**
 * S31 · Beschaffungsprojekt anlegen (R-PRJ-02; UX copy §5.8, U167; Handoff §5.3: no Figma frame,
 * the S13 dialog rules as S12): „Name des Projekts“ with its hint, „Abbrechen“ and „Projekt anlegen“.
 *
 * - The first focus is the name field; Esc or „Abbrechen“ close the dialog and MatDialog returns
 *   the focus to „Projekt anlegen“ on S30.
 * - The name is checked on submit (Handoff §6.4): required and at most 120 characters after
 *   trimming, as the server checks it; the error summary takes the focus, the field shows its error.
 * - While the project is created the button keeps its label, shows the spinner and is
 *   `aria-disabled`. Success: S32 of the new project opens (its H1 takes the focus with the route
 *   change) and `projects.created` is announced.
 * - 403 `email-unconfirmed` (SEC-10): `pa-confirm-required-notice` shows and takes the focus; the
 *   name is kept for a second try after the confirmation.
 * - Any other failure shows the error notice `projects.create.error.title` (`role="alert"`): a 403
 *   for the role `projects.create.notAllowed`; `antiforgery` its text; everything else
 *   `projects.create.error`. A field error at `name` shows at the field. The request is marked
 *   `CALLER_SHOWS_FAILURES`.
 * - „Kriterienliste (optional)“ (`projects.create.criteria`, R-PRJ-02; S6-12) offers „Keine Liste“
 *   (preselected) and the saved lists of the organisation by name — the server lists no project's
 *   copy, so no list of another project is offered. Without saved lists, or when they cannot be
 *   read, the field is left out: the list can be assigned later. With a list chosen, the project
 *   gets its own copy right after it is created (`PUT …/criteria-configuration`). If that fails,
 *   S32 of the new project still opens and the general notice shows (owner decision S6-12 D5 (b)):
 *   the project exists, without a list.
 *
 * Below 600 px the dialog fills the screen and stacks its actions in DOM order (S13-M).
 *
 * @spec docs/SPEC_v2.md R-PRJ-02, R-PRJ-04; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants SEC-10, API-02, A11Y-09
 */
@Component({
  selector: 'pa-create-project-dialog',
  imports: [
    ConfirmRequiredNotice,
    ErrorSummary,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './create-project-dialog.html',
  styleUrl: './create-project-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CreateProjectDialog {
  /** Opens S31 with the first focus in the name field. */
  static open(dialog: MatDialog): MatDialogRef<CreateProjectDialog> {
    return dialog.open(CreateProjectDialog, {
      autoFocus: 'input',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
  }

  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject<MatDialogRef<CreateProjectDialog>>(MatDialogRef);
  private readonly lists = inject(ConfiguratorApiService);
  private readonly snackbar = inject(SnackbarService);
  private readonly injector = inject(Injector);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');

  /** The organisation the header shows when the dialog opens; the project is created for it (R-SHL-03). */
  private readonly organization = this.store.activeOrganization();
  /** `{{org}}` of `projects.create.notAllowed`. */
  protected readonly organizationName = this.organization?.name ?? '';

  protected readonly name = new FormControl('', { nonNullable: true, validators: [requiredName, nameLength] });
  protected readonly nameId = 'pa-create-project-name';
  protected readonly hintId = 'pa-create-project-name-hint';

  /**
   * The organisation list to copy into the project; {@link NO_LIST} for „Keine Liste“ — a real
   * value, because MatSelect shows an option with the value null as no choice at all.
   */
  protected readonly criteriaList = new FormControl(NO_LIST, { nonNullable: true });
  protected readonly noList = NO_LIST;
  protected readonly criteriaHintId = 'pa-create-project-criteria-hint';
  /** The saved lists of the organisation by name (German collation); empty hides the field. */
  protected readonly savedLists = signal<SavedListSummary[]>([]);

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly error = signal<CreateError | null>(null);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);

  private readonly submitted = signal(false);
  private destroyed = false;

  /** Field errors show after the first submit (Handoff §6.4), server errors at once. */
  protected readonly errorState: ErrorStateMatcher = {
    isErrorState: (control) => !!control?.invalid && (this.submitted() || !!control.hasError('server')),
  };

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
    if (this.organization !== null) void this.loadSavedLists(this.organization.id);
  }

  /** The lists „Kriterienliste (optional)“ offers; a failure leaves the field out (assigned later). */
  private async loadSavedLists(organizationId: string): Promise<void> {
    try {
      const lists = await this.lists.list(organizationId);
      this.savedLists.set([...lists].sort((a, b) => a.name.localeCompare(b.name, 'de')));
    } catch {
      this.savedLists.set([]);
    }
  }

  /** The message at the name field, also the line of the error summary (E12). */
  protected nameMessage(): TextKey | null {
    if (this.name.hasError('server')) return { key: 'errors.field.server', params: { message: this.name.getError('server') } };
    if (this.name.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('projects.create.name') as string } };
    }
    if (this.name.hasError('maxlength')) {
      return { key: 'errors.field.maxLength', params: { max: PROJECT_NAME_MAX_LENGTH, current: this.name.value.trim().length } };
    }
    return null;
  }

  protected errorKey(error: CreateError): string {
    switch (error) {
      case 'notAllowed':
        return 'projects.create.notAllowed';
      case 'antiforgery':
        return 'errors.antiforgery';
      default:
        return 'projects.create.error';
    }
  }

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (this.busy()) return;
    this.submitted.set(true);
    const message = this.nameMessage();
    if (message !== null) {
      this.summary.set([{ message, field: { id: this.nameId, labelKey: 'projects.create.name' } }]);
      return;
    }
    if (this.organization === null) {
      this.error.set('failed');
      return;
    }

    const name = this.name.value.trim();
    const listId = this.savedLists().some((list) => list.id === this.criteriaList.value) ? this.criteriaList.value : null;
    this.summary.set([]);
    this.error.set(null);
    this.busy.set(true);
    let id: string;
    // The project exists once created: a list that cannot be copied leaves it without one (D5 (b)).
    let listCopied = true;
    try {
      ({ id } = await this.store.createProject(this.organization.id, name));
      if (listId !== null) {
        listCopied = await this.store.assignCriteriaList(id, listId).then(
          () => true,
          () => false,
        );
      }
    } catch (error: unknown) {
      if (!this.destroyed) this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }

    this.dialogRef.close(id);
    await this.router.navigateByUrl(projectPath(id));
    void this.announcer.announce('projects.created', { name });
    if (!listCopied) this.snackbar.showError(this.translate.instant(GENERAL_ERROR_KEY) as string);
  }

  private refused(error: unknown): void {
    switch (problemTypeName(error)) {
      case 'email-unconfirmed':
        this.confirmationRequired.set(true);
        afterNextRender(() => this.confirmNotice()?.nativeElement.focus(), { injector: this.injector });
        return;
      case 'antiforgery':
        this.error.set('antiforgery');
        return;
      case 'validation': {
        const lines = applyServerErrors(error, (path) =>
          path === 'name' ? { control: this.name, id: this.nameId, labelKey: 'projects.create.name' } : undefined,
        );
        if (this.name.hasError('server')) {
          this.summary.set(lines.filter((line) => line.field?.id === this.nameId));
        } else {
          this.error.set('failed');
        }
        return;
      }
      default:
        this.error.set(error instanceof HttpErrorResponse && error.status === 403 ? 'notAllowed' : 'failed');
    }
  }
}
