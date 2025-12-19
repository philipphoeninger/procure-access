import { Component, inject, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { MatTabsModule } from '@angular/material/tabs';
import { MatListModule } from '@angular/material/list';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'pa-products-favorites',
  imports: [
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatListModule,
    MatTooltipModule
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
