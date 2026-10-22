// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, inject, model, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-update-user-dialog',
  imports: [
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    TranslatePipe
  ],
  templateUrl: './update-user-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './update-user-dialog.scss'
})
export class UpdateUserDialog {
  readonly dialogRef = inject(MatDialogRef<UpdateUserDialog>);
  readonly data = inject<{ property: string, value: string }>(MAT_DIALOG_DATA);
  readonly property = model(this.data.property);
  readonly value = model(this.data.value);

  onCancelClick(): void {
    this.dialogRef.close();
  }
}
