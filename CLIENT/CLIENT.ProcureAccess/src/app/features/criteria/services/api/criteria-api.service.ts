import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { httpAppConfig } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Criterion } from "../../models/criterion.model";
import { filterTypeValues } from "@app/features/filters/data/dummy-data";

@Injectable({ providedIn:'root' })
export class CriteriaApiService {
  constructor(private http: HttpClient) {}

  getCriteriaByFilterTypeIds(selectedFilterTypeIds: number[]): Promise<Criterion[]> {
    // return this.http.get<Criterion[]>(`${httpAppConfig.apiEndpoint}/Criteria`);
    let filterTypes = 
        filterTypeValues.filter(filterType => selectedFilterTypeIds.includes(filterType.id));
    let criteria: Criterion[] = [];
    filterTypes.forEach(type => {
        criteria = criteria.concat(type.criteria);
    });
    return lastValueFrom(of(criteria));
  }
}
