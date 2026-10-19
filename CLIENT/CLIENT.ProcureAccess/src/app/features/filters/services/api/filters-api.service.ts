import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom } from "rxjs";
import { CriteriaFilter } from "../../models/criteriaFilter.model";
import { FilterType } from "../../models/filterType.model";
import { TResult } from "@app/core/models/result.model";

@Injectable({ providedIn:'root' })
export class FiltersApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient) {}

  getAllFilterTypes(): Promise<TResult<FilterType[]>> {
    return lastValueFrom(
      this.http.get<TResult<FilterType[]>>(`${this.apiUrl}/FilterTypes`)
    );
  }

  getAllCriteriaFilters(): Promise<TResult<CriteriaFilter[]>> {
    return lastValueFrom(
      this.http.get<TResult<CriteriaFilter[]>>(`${this.apiUrl}/CriteriaFilters`)
    );
  }
}
