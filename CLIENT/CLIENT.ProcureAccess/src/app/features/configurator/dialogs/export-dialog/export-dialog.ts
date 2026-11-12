// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
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
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatRadioModule } from '@angular/material/radio';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { problemDetails, problemTypeName } from '@app/core/errors/problem-details';
import { FileSaver } from '@app/core/files/file-saver';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { ExportFormat } from '../../models/export';
import { ExportApiService } from '../../services/api/export-api.service';
import { includedCount } from '../../state/criteria-list-rules';
import { toExportRequest } from '../../state/export-request';

/** What the dialog shows below the notes; the error states keep the form (Handoff §5.1 S13-E). */
type ExportStatus =
  | { kind: 'ready' }
  | { kind: 'busy' }
  | { kind: 'failed' }
  | { kind: 'rateLimited'; minutes: number }
  | { kind: 'outdated' };

/** Panel class of the dialog: 600 px from 600 px, full screen below (`styles/main.scss`, Handoff §2.3). */
export const EXPORT_DIALOG_PANEL_CLASS = 'pa-dialog';

/**
 * S13 · Kriterienliste exportieren (R-LST-11; Handoff §5.1 S13 row, UX copy §5.6): file format
 * (XLSX preselected, or CSV), what is exported, the catalog release, the anonymous note while
 * nobody is signed in, the ⚖️ legal note as written, „Abbrechen“ and „Datei herunterladen“.
 *
 * - The first focus is the selected radio ({@link ExportDialog.open}); Esc or „Abbrechen“ close the
 *   dialog and MatDialog returns the focus to „Liste exportieren“.
 * - While the file is requested the button keeps its label, shows the 20 px spinner and is
 *   `aria-disabled` (Handoff §6.2). Success: the browser downloads the file, `criteria.export.success`
 *   is announced and the dialog closes.
 * - A failure shows an error notice above the actions (`role="alert"`, S13-E): `criteria.export.error`
 *   with „Erneut versuchen“ in place of „Datei herunterladen“; a 429 the same with `errors.rateLimit`
 *   and its minutes; `release-mismatch` `criteria.export.outdated` with „Schließen“ only, which takes
 *   the focus (owner decision O-S4-72 (a); reloading the list is a follow-up).
 * - The request is marked `CALLER_SHOWS_FAILURES`, so no snackbar or server page shows besides
 *   (O-S4-74 (a)); a request without an answer also turns the offline notice on.
 *
 * Below 600 px the dialog fills the screen (S13-M): title on top, content scrolls, actions fixed at
 * the bottom, stacked in DOM order.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2134-3825
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2021-225
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-1944 (error)
 * @spec docs/SPEC_v2.md R-LST-11, R-LST-12, §7.3, §8.3
 * @invariants EXP-05, API-05
 */
@Component({
  selector: 'pa-export-dialog',
  imports: [MatButtonModule, MatDialogModule, MatProgressSpinnerModule, MatRadioModule, Notice, TranslatePipe, TranslatePluralPipe],
  templateUrl: './export-dialog.html',
  styleUrl: './export-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ExportDialog {
  /** Opens S13 with the first focus on the selected format. */
  static open(dialog: MatDialog): MatDialogRef<ExportDialog> {
    return dialog.open(ExportDialog, {
      // The checked radio is the group's only tab stop; a selector also works on iOS, where the
      // CDK counts fewer elements as tabbable.
      autoFocus: 'input[type="radio"]:checked',
      panelClass: EXPORT_DIALOG_PANEL_CLASS,
      maxWidth: '100vw',
    });
  }

  private readonly store = inject(ProcureAccessStore);
  private readonly api = inject(ExportApiService);
  private readonly files = inject(FileSaver);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly dialogRef = inject<MatDialogRef<ExportDialog>>(MatDialogRef);
  private readonly injector = inject(Injector);
  private readonly closeButton = viewChild('closeButton', { read: ElementRef });

  protected readonly format = signal<ExportFormat>('xlsx');
  protected readonly status = signal<ExportStatus>({ kind: 'ready' });
  protected readonly release = computed(() => this.store.configurator.list.answer()?.release ?? '');
  /** Cards in the user's list, as the action bar counts them (`criteria.list.status.count`). */
  protected readonly included = computed(() =>
    includedCount(this.store.configurator.list.answer()?.criteria ?? [], this.store.configurator.list.decisions()),
  );
  /** As on S11: the legacy token lives in `localStorage`, `user` changes with every sign-in. */
  protected readonly signedIn = computed(() => {
    this.store.user();
    return this.store.isAuthenticated();
  });
  protected readonly busy = computed(() => this.status().kind === 'busy');
  /** Minutes of a 429 (`errors.rateLimit`); 0 in every other state. */
  protected readonly retryAfterMinutes = computed(() => {
    const status = this.status();
    return status.kind === 'rateLimited' ? status.minutes : 0;
  });
  /** After a failure the primary button retries (S13-E). */
  protected readonly failedBefore = computed(() => ['failed', 'rateLimited'].includes(this.status().kind));

  private destroyed = false;
  /** Whether the last failure left the button „Erneut versuchen“; kept while the retry runs. */
  private retrying = false;

  constructor() {
    inject(DestroyRef).onDestroy(() => (this.destroyed = true));
  }

  protected primaryLabelKey(): string {
    return this.failedBefore() || (this.busy() && this.retrying) ? 'common.actions.retry' : 'criteria.export.submit';
  }

  protected chooseFormat(format: ExportFormat): void {
    this.format.set(format);
  }

  protected async download(): Promise<void> {
    if (this.busy()) return;
    const answer = this.store.configurator.list.answer();
    const selection = this.store.configurator.list.selection();
    this.retrying = this.failedBefore();
    if (answer === null || selection === null) {
      this.status.set({ kind: 'failed' });
      return;
    }

    this.status.set({ kind: 'busy' });
    try {
      const file = await this.api.exportList(
        toExportRequest(this.format(), answer, selection, this.store.configurator.list.decisions()),
      );
      if (this.destroyed) return;
      this.files.save(file.content, file.fileName);
      void this.announcer.announce('criteria.export.success');
      this.dialogRef.close();
    } catch (error: unknown) {
      if (this.destroyed) return;
      this.status.set(statusOf(error));
      if (this.status().kind === 'outdated') {
        // „Datei herunterladen“ is gone: the focus moves to „Schließen“, which replaces it.
        afterNextRender(
          () => (this.closeButton()?.nativeElement as HTMLButtonElement | undefined)?.focus(),
          { injector: this.injector },
        );
      }
    }
  }
}

/** The dialog state of a failed export. */
function statusOf(error: unknown): ExportStatus {
  const type = problemTypeName(error);
  if (type === 'release-mismatch') return { kind: 'outdated' };
  const minutes = problemDetails(error)?.retryAfterMinutes;
  if (error instanceof HttpErrorResponse && error.status === 429 && typeof minutes === 'number' && minutes > 0) {
    return { kind: 'rateLimited', minutes };
  }
  return { kind: 'failed' };
}
