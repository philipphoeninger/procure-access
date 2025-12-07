import { Component, inject, signal, ViewChild, WritableSignal } from '@angular/core';
import { FormBuilder, Validators, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { SaveSearchDialog } from '../../dialogs/save-search-dialog/save-search-dialog';
import { CriteriaList } from '@app/features/criteria/list/criteria-list';
import { RouterModule } from '@angular/router';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { FiltersContainer } from '@app/features/filters/pages/filters-container/filters-container';
import { FilterTypeValue } from '@app/features/filters/models/filterTypeValue.model';

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
    CriteriaList,
    RouterModule,
    FiltersContainer
  ],
  templateUrl: './products-container.html',
  styleUrl: './products-container.scss'
})
export class ProductsContainer {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);

    private _formBuilder = inject(FormBuilder);

  selectedFiltersFormGroup = this._formBuilder.group<{ filterTypes: FilterTypeValue[]}>({
    filterTypes: []
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

    selectedFilterTypeValueIds: WritableSignal<number[]> = signal([]);

    fdsa(event: number[]) {
      this.selectedFilterTypeValueIds.set(event);
    }

    asdf() {
      this.store.setSelectedFilters(this.selectedFilterTypeValueIds());
      this.store.getCriteriaBySelectedFilterTypeValueIds();
    }
}
