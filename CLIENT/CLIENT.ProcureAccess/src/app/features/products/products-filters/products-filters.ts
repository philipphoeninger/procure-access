import { LiveAnnouncer } from '@angular/cdk/a11y';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, computed, inject, model, signal, viewChild, WritableSignal } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { appTypes, productParts, products, productTypes, testTypes } from '../data/dummy-data';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatListModule } from '@angular/material/list';
import { MatAccordion, MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AsyncPipe } from '@angular/common';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { map, startWith } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { ProductType } from '../models/productType.model';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'pa-products-filters',
  imports: [
    FormsModule,
    MatDividerModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatExpansionModule,
    MatFormFieldModule,
    MatAutocompleteModule,
    AsyncPipe,
    MatChipsModule,
    MatInputModule,
    ReactiveFormsModule
  ],
  templateUrl: './products-filters.html',
  styleUrl: './products-filters.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductsFilters {
  protected store = inject(ProcureAccessStore);

  readonly dialog = inject(MatDialog);
  private _snackBar = inject(MatSnackBar);

  protected appTypes = appTypes;
  protected productParts = productParts;
  protected testTypes = testTypes;

    accordion = viewChild.required(MatAccordion);


      filteredProductTypes: Observable<string[]>;
  ngOnInit() {

  }

  // TODO: make it a service
  showSnackbar(message: string) {
    this._snackBar.open(message, 'Close', {
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }


  // ------

    myControl = new FormControl('');

    readonly separatorKeysCodes: number[] = [ENTER, COMMA];
  readonly currentProductType = model('');
  readonly productTypes: WritableSignal<string[]> = signal([]);
  readonly allProductTypes: string[] = [];

//   readonly filteredProductTypes = computed(() => {
//     const currentProductType = this.myControl.value.toLowerCase();
//     return currentProductType
//       ? this.allProductTypes.filter(productType => productType.toLowerCase().includes(currentProductType))
//       : this.allProductTypes.slice();
//   });

  readonly announcer = inject(LiveAnnouncer);

    constructor() {
        productTypes.forEach(productType => {
            this.allProductTypes.push(productType.name);
        });
            this.filteredProductTypes = this.myControl.valueChanges.pipe(
      startWith(''),
      map(value => this._filter(value || '')),
    );
  }

    private _filter(value: string): string[] {
    const filterValue = value.toLowerCase();

    return this.allProductTypes.filter(productType => productType.toLowerCase().includes(filterValue));
  }

  add(event: MatChipInputEvent): void {
    const value = (event.value || '').trim();

    // Add our fruit
    if (value) {
      this.productTypes.update(productTypes => [...productTypes, value]);
    }

    // Clear the input value
    this.currentProductType.set('');
  }

  remove(productType: string): void {
    this.productTypes.update(productTypes => {
      const index = productTypes.indexOf(productType);
      if (index < 0) {
        return productTypes;
      }

      productTypes.splice(index, 1);
      this.announcer.announce(`Removed ${productType}`);
      return [...productTypes];
    });
  }

  selected(event: MatAutocompleteSelectedEvent): void {
    this.productTypes.update(productTypes => [...productTypes, event.option.viewValue]);
    this.currentProductType.set('');
    event.option.deselect();
  }
}
