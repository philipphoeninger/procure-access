import { LiveAnnouncer } from '@angular/cdk/a11y';
import { Component, inject, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { products } from '../data/dummy-data';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatSort, Sort, MatSortModule } from '@angular/material/sort';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { SaveSearchDialog } from '../dialogs/save-search-dialog/save-search-dialog';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { RouterModule } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'pa-products-list',
  imports: [
    FormsModule,
    MatDividerModule,
    MatTableModule, 
    MatButtonModule,
    MatSortModule,
    MatIconModule,
    RouterModule,
    MatTooltipModule
  ],
  templateUrl: './products-list.html',
  styleUrl: './products-list.scss'
})
export class ProductsList {
  protected store = inject(ProcureAccessStore);
  private _liveAnnouncer = inject(LiveAnnouncer);

  displayedColumns: string[] = ['actions', 'id', 'name', 'link', 'functionality', 'type'];
  dataSource = new MatTableDataSource(products);

  @ViewChild(MatSort)
  sort!: MatSort;

  readonly dialog = inject(MatDialog);

  constructor(protected snackbarService: SnackbarService) {}

  ngOnInit() {}

  ngAfterViewInit() {
    this.dataSource.sort = this.sort;
  }

    /** Announce the change in sort state for assistive technology. */
  announceSortChange(sortState: Sort) {
    // This example uses English messages. If your application supports
    // multiple language, you would internationalize these strings.
    // Furthermore, you can customize the message to add additional
    // details about the values being sorted.
    if (sortState.direction) {
      this._liveAnnouncer.announce(`Sorted ${sortState.direction}ending`);
    } else {
      this._liveAnnouncer.announce('Sorting cleared');
    }
  }

  linkCopied() {
    this.snackbarService.showInfo('Link was created and copied to clipboard!');
  }

  exportProductsFile(){
    const link = document.createElement('a');
    link.setAttribute('target', '_blank');
    link.setAttribute('href', './');
    link.setAttribute('download', `products.csv`);
    document.body.appendChild(link);
    link.click();
    link.remove();
}

  openProductsSearchDialog(): void {
    const dialogRef = this.dialog.open(SaveSearchDialog, {
      width: '420px'
    });

    dialogRef.afterClosed().subscribe((name: string) => {
      if (name !== undefined) {
        this.snackbarService.showInfo('The products were saved under: ' + name);
      }
    });
  }

  addProductFavorite(productId: string) {
    this.store.addProductFavorite(productId);
    this.snackbarService.showInfo('Saved the product to your favorites!')
  }
}
