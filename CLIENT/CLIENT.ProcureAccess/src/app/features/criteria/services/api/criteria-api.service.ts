import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { httpAppConfig } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Criterion } from "../../models/criterion.model";

@Injectable({ providedIn:'root' })
export class CriteriaApiService {
  constructor(private http: HttpClient) {}

  getAllCriteria(): Promise<Criterion[]> {
    return lastValueFrom(
        this.http.get<Criterion[]>(`${httpAppConfig.apiEndpoint}/Criteria`)
    );
  }

  getCriteriaByCriteriaFilterIds(selectedFilterTypeIds: number[]): Promise<Criterion[]> {
    return lastValueFrom(
      this.http.get<Criterion[]>(
        `${httpAppConfig.apiEndpoint}/Criteria/byCriteriaFilterIds`, {
          params: {
            criteriaFilterIds: selectedFilterTypeIds
          }
        }
      )
    );
  }
}
