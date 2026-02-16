import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { httpAppConfig } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { criteriaFilters, filterTypes } from "@app/features/filters/data/dummy-data";
import { CriteriaFilter } from "../../models/criteriaFilter.model";
import { FilterType } from "../../models/filterType.model";

@Injectable({ providedIn:'root' })
export class FiltersApiService {
  constructor(private http: HttpClient) {}

  getAllFilterTypes(): Promise<FilterType[]> {
    return lastValueFrom(
      this.http.get<FilterType[]>(`${httpAppConfig.apiEndpoint}/FilterTypes`)
    );
  }

  getAllCriteriaFilters(): Promise<CriteriaFilter[]> {
    let allCriteriaFilters = criteriaFilters;
    return lastValueFrom(of(allCriteriaFilters));
  }
}
