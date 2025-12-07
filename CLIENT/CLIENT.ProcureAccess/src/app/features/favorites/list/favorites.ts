import { Component, inject, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { products } from '../../products/data/dummy-data';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';

@Component({
  selector: 'pa-products-favorites',
  imports: [
    FormsModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './favorites.html',
  styleUrl: './favorites.scss'
})
export class Favorites {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);

  constructor(protected snackbarService: SnackbarService) {}

  ngOnInit() {}

  ngAfterViewInit() {}

  showSnackbar(message: string) {
    this.snackbarService.showInfo(message);
  }
}
