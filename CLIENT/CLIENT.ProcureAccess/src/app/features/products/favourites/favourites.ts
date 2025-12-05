import { Component, inject, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { products } from '../data/dummy-data';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'pa-products-favourites',
  imports: [
    FormsModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './favourites.html',
  styleUrl: './favourites.scss'
})
export class Favourites {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);
  private _snackBar = inject(MatSnackBar);

  ngOnInit() {}

  ngAfterViewInit() {}

  // TODO: make it a service
  showSnackbar(message: string) {
    this._snackBar.open(message, 'Close', {
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
}
