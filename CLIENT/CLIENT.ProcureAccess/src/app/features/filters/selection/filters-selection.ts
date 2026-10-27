// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  ChangeDetectionStrategy, Component, computed,
  effect, inject, output, signal, viewChild, ViewChildren, WritableSignal
} from '@angular/core';
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
    readonly markedCriteriaFilters: WritableSignal<CriteriaFilter[]> = signal([]);
    markedFilterTypesOut = output<CriteriaFilter[]>();

    unavailableOptionIds = computed(() =>
      Array.from(
        new Set(
          this.markedCriteriaFilters()
            .flatMap(x => x.exclusions)
            .map(x => x?.id)
        )
      )
    );

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
          let selectedCriteriaFilterIds = this.markedCriteriaFilters();
          let index = selectedCriteriaFilterIds.findIndex(x => x.id == this.currentProductType()!.id);
          selectedCriteriaFilterIds.splice(index, 1);
          this.markedCriteriaFilters.set(selectedCriteriaFilterIds);
          this.currentProductType.set(null);
          // this.listOptions.forEach(x => x.selected = false);
        }),
      );

      // outputs
      effect(() => {
        this.markedFilterTypesOut.emit(this.markedCriteriaFilters());
      })
    }

    async ngOnInit() {
        if (this.store.filterTypes().length === 0)
            await this.store.loadFilters();

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
        let selectedCriteriaFilterIds = this.markedCriteriaFilters();
        let productType = this.store.getCriteriaFilterByName(event.option.value);
        if (!productType) return; //gate
        if (!event.option.selected) {
            let index = selectedCriteriaFilterIds.findIndex(x => x.id == productType.id);
            selectedCriteriaFilterIds.splice(index, 1);
            this.markedCriteriaFilters.set(selectedCriteriaFilterIds);
            this.currentProductType.set(null);
        } else {
            this.markedCriteriaFilters.set([productType]);
            // this.listOptions.forEach(x => x.selected = false);
            this.currentProductType.set(productType);
        }
    }

    changeCriteriaFilterMark(event: boolean, criteriaFilter: CriteriaFilter) {
      this.markedCriteriaFilters.update(x =>
        event
          ? [...x, criteriaFilter]
          : x.filter(y => y !== criteriaFilter)
      );
    }
}
