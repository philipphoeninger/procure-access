import { ChangeDetectionStrategy, Component, computed, inject, output, signal, viewChild, ViewChildren, WritableSignal } from '@angular/core';
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule, MatListOption } from '@angular/material/list';
import { MatAccordion, MatExpansionModule } from '@angular/material/expansion';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { AsyncPipe } from '@angular/common';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { map, Observable, startWith, tap } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { LiveAnnouncer } from '@angular/cdk/a11y';
import { FiltersApiService } from '../services/api/filters-api.service';
import { EnFilterTypeName } from '../models/filterTypes.enum';
import { CriteriaFilter } from '../models/criteriaFilter.model';
import { TranslatePipe } from '@ngx-translate/core';
import { ProductType } from '@app/features/products/models/productType.model';
import { unionDistinct } from '../util/union-distinct';

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

    protected listOptions = ViewChildren(MatListOption);

    EnFilterTypeName = EnFilterTypeName;

    accordion = viewChild.required(MatAccordion);

    myControl = new FormControl('');
    allProductTypes = this.store.productTypes;
    filteredProductTypes: Observable<string[]>;

    readonly separatorKeysCodes: number[] = [ENTER, COMMA];
    readonly currentProductType: WritableSignal<ProductType | null> 
        = signal(null);
    // readonly productTypes: WritableSignal<string[]> = signal([]);
    // allProductTypes: string[] = [];

    allFilterTypes = signal<Record<string, CriteriaFilter[]>>({});

    // remaining filters:
    readonly selectedCriteriaFilterIds: WritableSignal<number[]> = signal([]);
    selectedFilterTypeIdsOut = output<number[]>();
    unavailableOptionIds = computed(() => {
        let selectedCriteriaFilterIds = this.selectedCriteriaFilterIds();
        let unavailableOptions = 
            this.store.getCriteriaFilterExclusionsByFilterIds(
                selectedCriteriaFilterIds);
        return unionDistinct(unavailableOptions);
    });

    //   readonly filteredProductTypes = computed(() => {
    //     const currentProductType = this.myControl.value.toLowerCase();
    //     return currentProductType
    //       ? this.allProductTypes.filter(productType => productType.toLowerCase().includes(currentProductType))
    //       : this.allProductTypes.slice();
    //   });

    readonly announcer = inject(LiveAnnouncer);

    constructor() {
        this.filteredProductTypes = this.myControl.valueChanges.pipe(
            startWith(''),
            map(value => this._filter(value || '')),
            tap(() => {
                if (!this.currentProductType()) return; //gate
                let selectedCriteriaFilterIds = this.selectedCriteriaFilterIds();
                let index = selectedCriteriaFilterIds.findIndex(x => x == this.currentProductType()!.id);
                selectedCriteriaFilterIds.splice(index, 1);
                this.selectedCriteriaFilterIds.set(selectedCriteriaFilterIds);
                this.currentProductType.set(null);
                // this.listOptions.forEach(x => x.selected = false);
                console.log(this.selectedCriteriaFilterIds());
            }),
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

    private _filter(value: string): string[] {
        const filterValue = value.toLowerCase();

        return this.allProductTypes()
            .filter(productType => 
                productType.name
                    .toLowerCase()
                    .includes(filterValue))
            .map(x => x.name);
    }

    selected(event: MatAutocompleteSelectedEvent): void {
        let selectedCriteriaFilterIds = this.selectedCriteriaFilterIds();
        let productType = this.store.getCriteriaFilterByName(event.option.value);
        if (!productType) return; //gate
        if (!event.option.selected) {
            let index = selectedCriteriaFilterIds.findIndex(x => x == productType.id);
            selectedCriteriaFilterIds.splice(index, 1);
            this.selectedCriteriaFilterIds.set(selectedCriteriaFilterIds);
            this.currentProductType.set(null);
        } else {
            this.selectedCriteriaFilterIds.set([productType.id]);
            // this.listOptions.forEach(x => x.selected = false);
            this.currentProductType.set(productType);
        }
        console.log(this.selectedCriteriaFilterIds());
    }

    changeCriteriaFilterSelection(event: boolean, filterTypeId: number) {
        if (!event) {
            this.selectedCriteriaFilterIds.update(asdf => asdf.filter(i => i !== filterTypeId));
        } else {
            this.selectedCriteriaFilterIds.update(asdf => [...asdf, filterTypeId]);
        }
        this.selectedFilterTypeIdsOut.emit(this.selectedCriteriaFilterIds());
    }
}
