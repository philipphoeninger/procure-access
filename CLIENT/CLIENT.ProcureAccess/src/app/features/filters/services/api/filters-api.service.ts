import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { httpAppConfig } from "@app/app.config";
import { lastValueFrom } from "rxjs";
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
    return lastValueFrom(
      this.http.get<CriteriaFilter[]>(`${httpAppConfig.apiEndpoint}/CriteriaFilters`)
    );
  }
}
