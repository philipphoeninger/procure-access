import { Component, inject, ViewChild } from '@angular/core';
import { FormBuilder, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ProductsFilters } from '../../products-filters/products-filters';
import { ProductsList } from '../../list/products-list';
import { SaveSearchDialog } from '../../dialogs/save-search-dialog/save-search-dialog';
import { CriteriaList } from '@app/features/criteria/list/criteria-list';

@Component({
  selector: 'pa-products-container',
  imports: [
    FormsModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    MatStepperModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    ProductsFilters,
    ProductsList,
    CriteriaList
  ],
  templateUrl: './products-container.html',
  styleUrl: './products-container.scss'
})
export class ProductsContainer {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);
  private _snackBar = inject(MatSnackBar);

    private _formBuilder = inject(FormBuilder);

  firstFormGroup = this._formBuilder.group({
    firstCtrl: ['', Validators.required],
  });
  secondFormGroup = this._formBuilder.group({
    secondCtrl: ['', Validators.required],
  });

  constructor(protected snackbarService: SnackbarService) {}

  ngOnInit() {}

    linkCopied() {
      this.snackbarService.showInfo('Link was copied to clipboard!');
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

  // TODO: make it a service
  showSnackbar(message: string) {
    this._snackBar.open(message, 'Close', {
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
}
