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
import { FiltersApiService } from '../services/api/filters-api.service';
import { EnFilterTypeName } from '../models/filterTypes.enum';
import { FilterType } from '../models/filterType.model';
import { CriteriaFilter } from '../models/criteriaFilter.model';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-filters-selection',
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
    ReactiveFormsModule,
    TranslatePipe
  ],
  templateUrl: './filters-selection.html',
  styleUrl: './filters-selection.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FiltersSelection {
    protected store = inject(ProcureAccessStore);
    protected filtersApiService = inject(FiltersApiService);

    EnFilterTypeName = EnFilterTypeName;

    accordion = viewChild.required(MatAccordion);

    filteredProductTypes: Observable<string[]>;


    constructor() {
        this.filteredProductTypes = this.myControl.valueChanges.pipe(
            startWith(''),
            map(value => this._filter(value || '')),
        );
    }

    ngOnInit() {
        if (this.store.filterTypes().length === 0) 
            this.store.loadFilters();

        // this.filtersApiService.getAllCriteriaFilters().then((allFilters) => {
        //     let productFilterTypes = 
        //         allFilters.filter(x => x.filterType.name == EnFilterType.productType);
        //     let productFilterTypesNames = productFilterTypes.map(x => x.name);
        //     this.allProductTypes = productFilterTypesNames;

        //     let appFilterTypes = allFilters.filter(x => x.filterType.name == EnFilterType.appType);
        //     let testFilterTypes = allFilters.filter(x => x.filterType.name == EnFilterType.testType);
        //     let productPartFilterTypes = allFilters.filter(x => x.filterType.name == EnFilterType.productPart);

        //     let updateAllFilterTypes: Record<string, CriteriaFilter[]> = {};
        //     updateAllFilterTypes[EnFilterType.productType] = productFilterTypes;
        //     updateAllFilterTypes[EnFilterType.appType] = appFilterTypes;
        //     updateAllFilterTypes[EnFilterType.testType] = testFilterTypes;
        //     updateAllFilterTypes[EnFilterType.productPart] = productPartFilterTypes;

        //     this.allFilterTypes.set(updateAllFilterTypes);
        // });

        // this.filtersApiService.getAllFilterTypes().then((allFilterTypes) => {
        //     this.filterTypes.set(allFilterTypes);
        // })
    }

    // ------

    myControl = new FormControl('');

    readonly separatorKeysCodes: number[] = [ENTER, COMMA];
    readonly currentProductType = model('');
    readonly productTypes: WritableSignal<string[]> = signal([]);
    allProductTypes: string[] = [];

    allFilterTypes = signal<Record<string, CriteriaFilter[]>>({});

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

    private _filter(value: string): string[] {
        const filterValue = value.toLowerCase();

        return this.allProductTypes.filter(productType => productType.toLowerCase().includes(filterValue));
    }

    add(event: MatChipInputEvent): void {
        const value = (event.value || '').trim();

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
