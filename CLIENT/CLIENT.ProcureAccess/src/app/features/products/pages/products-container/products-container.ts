// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, inject, signal, ViewChild, WritableSignal, ChangeDetectionStrategy } from '@angular/core';
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
import { Router, RouterModule } from '@angular/router';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CriteriaFilter } from '@app/features/filters/models/criteriaFilter.model';
import { TranslatePipe } from '@ngx-translate/core';
import { FiltersSelection } from '@features/filters/selection/filters-selection';

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
    MatTooltipModule,
    TranslatePipe,
    FiltersSelection
  ],
  templateUrl: './products-container.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './products-container.scss'
})
export class ProductsContainer {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);

  private _formBuilder = inject(FormBuilder);

  selectedCriteriaFiltersFormGroup = this._formBuilder.group<{ criteriaFilter: CriteriaFilter[]}>({
    criteriaFilter: []
  });
  secondFormGroup = this._formBuilder.group({
    secondCtrl: ['', Validators.required],
  });

  constructor(
    protected snackbarService: SnackbarService,
    private router: Router
  ) {}

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

    markedCriteriaFilters: WritableSignal<CriteriaFilter[]> = signal([]);

    enterFilters() {
      this.store.setSelectedCriteriaFilters(this.markedCriteriaFilters());
      this.store.getCriteriaBySelectedCriteriaFilterIds(this.store.selectedCriteriaFiltersIds());
    }

    async showProductsByCriteriaFilters() {
      await this.store.getProductsBySelectedCriteriaFilterIds(this.store.selectedCriteriaFiltersIds());
      this.router.navigateByUrl('/products');
    }
}
