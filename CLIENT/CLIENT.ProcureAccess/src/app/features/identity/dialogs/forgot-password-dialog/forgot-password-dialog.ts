import { Component, inject, model } from '@angular/core';
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

@Component({
  selector: 'pa-forgot-password-dialog',
  imports: [
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose
  ],
  templateUrl: './forgot-password-dialog.html',
  styleUrl: './forgot-password-dialog.scss'
})
export class ForgotPasswordDialog {
  readonly dialogRef = inject(MatDialogRef<ForgotPasswordDialog>);
  readonly data = inject<{ email: string }>(MAT_DIALOG_DATA);
  readonly email = model(this.data.email);

  onCancelClick(): void {
    this.dialogRef.close();
  }
}
