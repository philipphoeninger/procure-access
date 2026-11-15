// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslatePipe } from '@ngx-translate/core';

/** The texts of a confirmation: UX copy keys and the parameters of the text. */
export interface ConfirmDialogData {
  titleKey: string;
  bodyKey: string;
  bodyParams?: Record<string, string>;
  confirmKey: string;
}

/**
 * A confirmation before an action that cannot be undone (Handoff §5.3 „as S36“, S13 dialog rules):
 * title, text, „Abbrechen“ (`common.actions.cancel`) and the action. Opens with the focus on the
 * title, so the question is read first; Escape and „Abbrechen“ close it with `false`, the action
 * with `true`, and MatDialog returns the focus to the opener. First users: removing a member and
 * leaving an organisation on S21 (S5-16b, UX copy U129, U130).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.3, §2.3
 */
@Component({
  selector: 'pa-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule, TranslatePipe],
  template: `
    <h2 mat-dialog-title>{{ data.titleKey | translate }}</h2>
    <mat-dialog-content>
      <p class="pa-confirm-dialog__body">{{ data.bodyKey | translate: data.bodyParams }}</p>
    </mat-dialog-content>
    <mat-dialog-actions class="pa-confirm-dialog__actions">
      <button matButton type="button" [mat-dialog-close]="false">{{ 'common.actions.cancel' | translate }}</button>
      <button matButton="filled" type="button" [mat-dialog-close]="true">{{ data.confirmKey | translate }}</button>
    </mat-dialog-actions>
  `,
  styles: `
    .pa-confirm-dialog__body {
      margin: 0;
      font: var(--mat-sys-body-large);
    }

    // DOM order = visual order (Handoff §2.2): below 600 px stacked, the action last (S10-M)
    .pa-confirm-dialog__actions {
      flex-direction: column;
      align-items: stretch;
      gap: var(--pa-space-xs);

      @media (min-width: 37.5em) {
        flex-direction: row;
        justify-content: flex-end;
      }
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmDialog {
  protected readonly data = inject<ConfirmDialogData>(MAT_DIALOG_DATA);

  /** Opens the dialog; resolves to true when the user confirmed. */
  static async confirm(dialog: MatDialog, data: ConfirmDialogData): Promise<boolean> {
    const ref: MatDialogRef<ConfirmDialog, boolean> = dialog.open(ConfirmDialog, {
      data,
      autoFocus: 'first-heading',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
    return new Promise((resolve) => ref.afterClosed().subscribe((confirmed) => resolve(confirmed === true)));
  }
}
