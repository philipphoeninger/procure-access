// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, DOCUMENT, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { Notice } from '@app/shared/components/notice/notice';

/** What S37 shows and copies. */
export interface ConflictDialogData {
  /** Who made the stored version (`editorDisplayName` of the 409); null when the server names nobody. */
  editorDisplayName: string | null;
  /** The user's unsaved input as plain text, built by {@link changesText}. */
  changes: string;
}

/** The text of the body: its id is the dialog's description. */
const BODY_ID = 'pa-conflict-dialog-body';

/** The text the general error shows when the browser refuses the clipboard. */
const GENERAL_ERROR_KEY = 'ERROR.GENERIC';

/**
 * S37 „Inzwischen geändert“ (R-CAN-11, API-03; Handoff §5.1, Figma `2056:2519`): opened when a
 * save of a shared resource answers 409 `conflict`, because someone else changed it since it was
 * read. An alert dialog with title, `conflict.body` with the other editor's name
 * (`conflict.body.unknown` without one, UX copy U188), the attention notice `conflict.reload.hint`
 * and two actions:
 *
 * - „Meine Änderungen kopieren“ (filled, the first focus) writes the user's unsaved input as plain
 *   text to the clipboard ({@link ConflictDialogData.changes}, UX copy U189) and announces
 *   `conflict.copied`; the dialog stays open. A browser that refuses the clipboard (an insecure
 *   origin) gets the general error.
 * - „Neue Fassung laden“ closes the dialog with `true`: the caller drops the input, reads the
 *   resource again and moves the focus to the H1.
 *
 * Escape only closes the dialog (`false`), and MatDialog returns the focus to „Speichern“; the
 * caller keeps saving blocked until the resource is read again ({@link EditConflict}). Below
 * 600 px the dialog fills the screen and stacks its actions in DOM order (S13-M).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2056-2519
 * @spec docs/SPEC_v2.md R-CAN-11; docs/design_handoff_procureaccess_v1.md §5.1
 * @invariants API-03, A11Y-01
 */
@Component({
  selector: 'pa-conflict-dialog',
  imports: [MatButtonModule, MatDialogModule, Notice, TranslatePipe],
  template: `
    <h2 mat-dialog-title>{{ 'conflict.title' | translate }}</h2>
    <mat-dialog-content class="pa-conflict-dialog__content">
      <p [id]="bodyId" class="pa-conflict-dialog__text">
        @if (data.editorDisplayName; as name) {
          {{ 'conflict.body' | translate: { name } }}
        } @else {
          {{ 'conflict.body.unknown' | translate }}
        }
      </p>
      <pa-notice kind="attention">
        <p class="pa-conflict-dialog__text">{{ 'conflict.reload.hint' | translate }}</p>
      </pa-notice>
    </mat-dialog-content>
    <mat-dialog-actions class="pa-conflict-dialog__actions">
      <button matButton type="button" [mat-dialog-close]="true">{{ 'conflict.reload' | translate }}</button>
      <button matButton="filled" type="button" class="pa-conflict-dialog__copy" (click)="copy()">
        {{ 'conflict.copy' | translate }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .pa-conflict-dialog__content {
      display: flex;
      flex-direction: column;
      gap: var(--pa-space-md);
    }

    .pa-conflict-dialog__text {
      margin: 0;
    }

    // DOM order = visual order (Handoff §2.2): below 600 px stacked, the first focus last (S13-M)
    .pa-conflict-dialog__actions {
      flex-direction: column;
      align-items: stretch;
      gap: var(--pa-space-xs);

      @media (min-width: 37.5em) {
        flex-direction: row;
        justify-content: flex-end;
        align-items: center;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConflictDialog {
  protected readonly data = inject<ConflictDialogData>(MAT_DIALOG_DATA);
  protected readonly bodyId = BODY_ID;

  private readonly announcer = inject(A11yAnnouncerService);
  private readonly snackbar = inject(SnackbarService);
  private readonly translate = inject(TranslateService);
  private readonly document = inject(DOCUMENT);

  /** Opens S37; it closes with `true` for „Neue Fassung laden“, with nothing for Escape. */
  static open(dialog: MatDialog, data: ConflictDialogData): MatDialogRef<ConflictDialog, boolean> {
    return dialog.open(ConflictDialog, {
      data,
      role: 'alertdialog',
      ariaDescribedBy: BODY_ID,
      autoFocus: '.pa-conflict-dialog__copy',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
  }

  /** Opens S37 and resolves to true when the user chose „Neue Fassung laden“. */
  static show(dialog: MatDialog, data: ConflictDialogData): Promise<boolean> {
    const ref = ConflictDialog.open(dialog, data);
    return new Promise((resolve) => ref.afterClosed().subscribe((reload) => resolve(reload === true)));
  }

  protected async copy(): Promise<void> {
    try {
      const clipboard = this.document.defaultView?.navigator.clipboard;
      if (clipboard === undefined) throw new Error('No clipboard');
      await clipboard.writeText(this.data.changes);
    } catch {
      this.snackbar.showError(this.translate.instant(GENERAL_ERROR_KEY) as string);
      return;
    }
    void this.announcer.announce('conflict.copied');
  }
}
