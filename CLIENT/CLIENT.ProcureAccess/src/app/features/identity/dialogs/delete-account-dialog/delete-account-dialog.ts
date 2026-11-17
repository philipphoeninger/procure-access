// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '@ngx-translate/core';

/** The address of the signed-in account, which the user has to type. */
export interface DeleteAccountDialogData {
  email: string;
}

/**
 * The confirmation of S61 „Konto endgültig löschen?“ (SPEC R-AUTH-10; UX copy §5.14
 * `account.delete.confirm.*`, U93, U159; Handoff §5.3 dialog rules).
 *
 * The user types the address of the account. Until it matches exactly — no case folding; an
 * e-mail field drops surrounding spaces itself — „Konto endgültig löschen“ is `aria-disabled`, stays focusable and does nothing, and
 * the reason `account.delete.confirm.unavailable` is visible below the buttons and linked with
 * `aria-describedby` (CLAUDE §9). An empty input never matches, also for an empty account address.
 * The dialog only confirms: it closes with `true`, and S61 deletes the account. „Konto behalten“
 * and Escape close it with `false`. It opens with the focus on its title.
 *
 * @spec docs/SPEC_v2.md R-AUTH-10, §8.13; docs/design_handoff_procureaccess_v1.md §5.3
 */
@Component({
  selector: 'pa-delete-account-dialog',
  imports: [MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule, TranslatePipe],
  templateUrl: './delete-account-dialog.html',
  styleUrl: './delete-account-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeleteAccountDialog {
  private readonly dialogRef = inject<MatDialogRef<DeleteAccountDialog, boolean>>(MatDialogRef);
  private readonly data = inject<DeleteAccountDialogData>(MAT_DIALOG_DATA);

  protected readonly typed = signal('');
  protected readonly matches = computed(() => this.typed() !== '' && this.typed() === this.data.email);

  protected readonly inputId = 'pa-delete-account-email';
  protected readonly reasonId = 'pa-delete-account-reason';

  /** Opens the dialog; resolves to true when the user typed the address and confirmed. */
  static confirm(dialog: MatDialog, data: DeleteAccountDialogData): Promise<boolean> {
    const ref: MatDialogRef<DeleteAccountDialog, boolean> = dialog.open(DeleteAccountDialog, {
      data,
      autoFocus: 'first-heading',
      panelClass: 'pa-dialog',
      maxWidth: '100vw',
    });
    return new Promise((resolve) => ref.afterClosed().subscribe((confirmed) => resolve(confirmed === true)));
  }

  protected input(event: Event): void {
    this.typed.set((event.target as HTMLInputElement).value);
  }

  /** The button and Enter in the field; does nothing until the address matches. */
  protected confirm(event: Event): void {
    event.preventDefault();
    if (this.matches()) {
      this.dialogRef.close(true);
    }
  }
}
