import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Criterion } from "../../models/criterion.model";
import { TResult } from "@app/core/models/result.model";

@Injectable({ providedIn:'root' })
export class CriteriaApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient) {}

  getAllCriteria(): Promise<TResult<Criterion[]>> {
    return lastValueFrom(
        this.http.get<TResult<Criterion[]>>(`${this.apiUrl}/Criteria`)
    );
  }

  getCriteriaByCriteriaFilterIds(selectedFilterTypeIds: number[]): Promise<Criterion[]> {
    return lastValueFrom(
      this.http.get<Criterion[]>(
        `${this.apiUrl}/Criteria/byCriteriaFilterIds`, {
          params: {
            criteriaFilterIds: selectedFilterTypeIds
          }
        }
      )
    );
  }
}
