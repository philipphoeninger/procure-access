import { Component, computed, inject, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'pa-delete-account-dialog',
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
  templateUrl: './delete-account-dialog.html',
  styleUrl: './delete-account-dialog.scss'
})
export class DeleteAccountDialog {
  readonly dialogRef = inject(MatDialogRef<DeleteAccountDialog>);
  readonly data = inject<{ email: string }>(MAT_DIALOG_DATA);
  readonly email = model(this.data.email);

  inputEmail = model('');

  deletionDisabled = computed(() => this.inputEmail() !== this.email());
}
