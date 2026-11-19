// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { afterNextRender, ChangeDetectionStrategy, Component, DestroyRef, ElementRef, inject, Injector, signal, viewChild } from '@angular/core';
import { AbstractControl, FormControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { ErrorStateMatcher } from '@angular/material/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ErrorSummaryItem, TextKey } from '@app/core/errors/error-summary-item';
import { applyServerErrors, problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { ConfirmRequiredNotice } from '@app/features/identity/components/confirm-required-notice/confirm-required-notice';
import { ConflictDialog } from '@app/shared/components/conflict-dialog/conflict-dialog';
import { changesText, ChangedField, EditConflict } from '@app/shared/components/conflict-dialog/edit-conflict';
import { ErrorSummary } from '@app/shared/components/error-summary/error-summary';
import { Notice } from '@app/shared/components/notice/notice';

import { Project, PROJECT_NAME_MAX_LENGTH, PROJECT_STATUSES, ProjectChange, ProjectStatus, statusKey } from '../../models/projects';

function requiredName(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length === 0 ? { required: true } : null;
}

function nameLength(control: AbstractControl<string>): ValidationErrors | null {
  return control.value.trim().length > PROJECT_NAME_MAX_LENGTH ? { maxlength: true } : null;
}

/** A failure the dialog shows in its error notice above the actions (Handoff §5.1 S13-E). */
type ChangeError = 'failed' | 'archived' | 'antiforgery';

/** The dialog closes with this when the user chose „Neue Fassung laden“ in S37: S32 reads the project again. */
export const RELOAD_PROJECT = 'reload';

/** What the dialog closes with: the changed project, {@link RELOAD_PROJECT}, or nothing. */
export type EditProjectResult = Project | typeof RELOAD_PROJECT;

/**
 * Dialog „Projekt bearbeiten“ of S32 (R-PRJ-05; owner decision S6-14 D1 (a), UX copy v44; Handoff
 * §5.3: no Figma frame, the S13 dialog rules as S31): „Name des Projekts“ and „Stand“ („Laufend“,
 * „Abgeschlossen“, „Archiviert“ as a radio group), „Abbrechen“ and „Speichern“.
 *
 * - The first focus is the name field (for an archived project the chosen status); Esc or
 *   „Abbrechen“ close the dialog and MatDialog returns the focus to „Projekt bearbeiten“.
 * - An archived project is read-only except its status (R-PRJ-05, owner decision S6-11 2 (a)): the
 *   dialog shows `projects.archived.note` instead of the name field and sends only the status.
 * - The name is checked on submit as in S31 (required, ≤ 120 after trimming); the error summary
 *   takes the focus. Only what changed is sent; nothing changed closes the dialog without a request.
 * - While saving, „Speichern“ shows the spinner and is `aria-disabled`. Success closes the dialog
 *   with the changed project and announces `projects.edit.saved`.
 * - The change names the project's tag (`If-Match`, API-03). 403 `email-unconfirmed` (SEC-10)
 *   shows `pa-confirm-required-notice`, which takes the focus; 409 `project-archived` (archived
 *   meanwhile) shows `projects.archived.note`; `antiforgery` its text; a field error at `name` shows
 *   at the field; any other failure `projects.edit.error` under the title
 *   `projects.edit.error.title`.
 * - 409 `conflict` (someone else changed the project since it was read; API-03, R-CAN-11) opens
 *   S37 over this dialog with the changed name and status as text (S6-21). „Neue Fassung laden“
 *   closes both with {@link RELOAD_PROJECT}; Escape returns to this dialog, whose „Speichern“ then
 *   opens S37 again without a request until the project is read again.
 *
 * Below 600 px the dialog fills the screen and stacks its actions in DOM order (S13-M).
 *
 * @spec docs/SPEC_v2.md R-PRJ-05; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants SEC-10, API-02, A11Y-09
 */
@Component({
  selector: 'pa-edit-project-dialog',
  imports: [
    ConfirmRequiredNotice,
    ErrorSummary,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    Notice,
    ReactiveFormsModule,
    TranslatePipe,
  ],
  templateUrl: './edit-project-dialog.html',
  styleUrl: './edit-project-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditProjectDialog {
  /** Opens the dialog for `project`; it closes with the changed project, or nothing. */
  static open(dialog: MatDialog, project: Project): MatDialogRef<EditProjectDialog, EditProjectResult> {
    return dialog.open(EditProjectDialog, {
      data: project,
      // Archived: no name field; the focus goes to the chosen status, as Tab would reach it.
      autoFocus: project.status === 'archived' ? '.mat-mdc-radio-checked input' : 'input',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
  }

  private readonly project = inject<Project>(MAT_DIALOG_DATA);
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);
  private readonly dialogRef = inject<MatDialogRef<EditProjectDialog, EditProjectResult>>(MatDialogRef);
  private readonly dialog = inject(MatDialog);
  private readonly injector = inject(Injector);
  private readonly confirmNotice = viewChild<ElementRef<HTMLElement>>('confirmNotice');

  /** Archived: only the status may change (R-PRJ-05). */
  protected readonly archived = this.project.status === 'archived';
  protected readonly statuses = PROJECT_STATUSES;
  protected readonly statusKey = statusKey;

  protected readonly name = new FormControl(this.project.name, { nonNullable: true, validators: [requiredName, nameLength] });
  protected readonly status = new FormControl<ProjectStatus>(this.project.status, { nonNullable: true });
  protected readonly nameId = 'pa-edit-project-name';
  protected readonly statusLabelId = 'pa-edit-project-status-label';

  protected readonly busy = signal(false);
  protected readonly summary = signal<ErrorSummaryItem[]>([]);
  protected readonly error = signal<ChangeError | null>(null);
  /** True after the server refused the unconfirmed address (SEC-10). */
  protected readonly confirmationRequired = signal(false);

  private readonly submitted = signal(false);
  private readonly conflict = new EditConflict();
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
    if (this.archived) return null;
    if (this.name.hasError('server')) return { key: 'errors.field.server', params: { message: this.name.getError('server') } };
    if (this.name.hasError('required')) {
      return { key: 'errors.field.required', params: { field: this.translate.instant('projects.create.name') as string } };
    }
    if (this.name.hasError('maxlength')) {
      return { key: 'errors.field.maxLength', params: { max: PROJECT_NAME_MAX_LENGTH, current: this.name.value.trim().length } };
    }
    return null;
  }

  protected errorKey(error: ChangeError): string {
    switch (error) {
      case 'archived':
        return 'projects.archived.note';
      case 'antiforgery':
        return 'errors.antiforgery';
      default:
        return 'projects.edit.error';
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

    const change: ProjectChange = {};
    const name = this.name.value.trim();
    if (!this.archived && name !== this.project.name) change.name = name;
    if (this.status.value !== this.project.status) change.status = this.status.value;
    if (change.name === undefined && change.status === undefined) {
      this.dialogRef.close();
      return;
    }

    this.summary.set([]);
    this.error.set(null);
    // After a conflict nothing is sent until the project is read again (S37)
    if (this.conflict.blocked) {
      await this.showConflict(change);
      return;
    }
    this.busy.set(true);
    let changed: Project;
    try {
      changed = await this.store.changeProject(this.project.id, change, this.project.etag);
    } catch (error: unknown) {
      this.busy.set(false);
      if (this.destroyed) return;
      if (this.conflict.caught(error)) await this.showConflict(change);
      else this.refused(error);
      return;
    } finally {
      this.busy.set(false);
    }

    this.dialogRef.close(changed);
    void this.announcer.announce('projects.edit.saved');
  }

  /** S37 with the changed fields; „Neue Fassung laden“ closes this dialog too, so S32 reads the project again. */
  private async showConflict(change: ProjectChange): Promise<void> {
    const fields: ChangedField[] = [];
    const label = (key: string) => this.translate.instant(key) as string;
    if (change.name !== undefined) fields.push({ label: label('projects.create.name'), value: change.name });
    if (change.status !== undefined) fields.push({ label: label('projects.status.label'), value: label(statusKey(change.status)) });
    const reload = await ConflictDialog.show(this.dialog, {
      editorDisplayName: this.conflict.editorDisplayName,
      changes: changesText([{ fields }]),
    });
    if (reload && !this.destroyed) this.dialogRef.close(RELOAD_PROJECT);
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
      case 'project-archived':
        this.error.set('archived');
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
        this.error.set('failed');
    }
  }
}
