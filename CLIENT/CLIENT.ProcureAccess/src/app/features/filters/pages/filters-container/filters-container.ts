import { ChangeDetectionStrategy, Component, inject, input, model, output, signal, viewChild, WritableSignal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatAccordion, MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AsyncPipe } from '@angular/common';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { map, Observable, startWith } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { filterTypes } from '../../data/dummy-data';
import { FilterTypeValue } from '../../models/filterTypeValue.model';

@Component({
  selector: 'pa-filters-container',
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
  templateUrl: './filters-container.html',
  styleUrl: './filters-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FiltersContainer {
    protected store = inject(ProcureAccessStore);

    filterTypes = filterTypes;

    accordion = viewChild.required(MatAccordion);

    formGroup1 = input.required<any>();

    filteredProductTypes: Observable<string[]>;

    ngOnInit() {}

    // ------

    myControl = new FormControl('');

    readonly separatorKeysCodes: number[] = [ENTER, COMMA];
    readonly currentProductType = model('');
    readonly productTypes: WritableSignal<string[]> = signal([]);
    readonly allProductTypes: string[] = [];

    // remaining filters:
    readonly selectedFilterTypeIds: WritableSignal<number[]> = signal([]);
    selectedFilterTypeIdsOut = output<number[]>();

    //   readonly filteredProductTypes = computed(() => {
    //     const currentProductType = this.myControl.value.toLowerCase();
    //     return currentProductType
    //       ? this.allProductTypes.filter(productType => productType.toLowerCase().includes(currentProductType))
    //       : this.allProductTypes.slice();
    //   });

    readonly announcer = inject(LiveAnnouncer);

    constructor() {
        filterTypes[0].values.forEach(productType => {
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

    changeFilterTypeSelection(event: boolean, filterTypeId: number) {
        if (!event) {
            this.selectedFilterTypeIds.update(asdf => asdf.filter(i => i !== filterTypeId));
        } else {
            this.selectedFilterTypeIds.update(asdf => [...asdf, filterTypeId]);
        }
        this.selectedFilterTypeIdsOut.emit(this.selectedFilterTypeIds());
    }
}
