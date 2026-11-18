// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpClient } from "@angular/common/http";
import { inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom } from "rxjs";
import { Product } from "../../models/product.model";
import { TResult } from "@app/core/models/result.model";

@Injectable({ providedIn:'root' })
export class ProductsApiService {
  private readonly apiUrl = inject(API_URL);
  private readonly http = inject(HttpClient);

  getAllProducts(): Promise<TResult<Product[]>> {
    // let allProducts = products;
    // return lastValueFrom(of(allProducts));
    return lastValueFrom(
        this.http.get<TResult<Product[]>>(`${this.apiUrl}/Products`)
    );
  }

  getProductsByCriteriaFilterIds(selectedFilterTypeIds: number[]): Promise<TResult<Product[]>> {
    return lastValueFrom(
      this.http.get<TResult<Product[]>>(
        `${this.apiUrl}/Products/byCriteriaFilterIds`, {
          params: {
            criteriaFilterIds: selectedFilterTypeIds
          }
        }
      )
    );
  }

//   getProductById(productId: number): Observable<Product> {
//     return this.http.get<FileItemResponseModel>(
//       `${httpAppConfig.apiEndpoint}/FileItems/withPath/${productId}`,
//     );
//   }
}
