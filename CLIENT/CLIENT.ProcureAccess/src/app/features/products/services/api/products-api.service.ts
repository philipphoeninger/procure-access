import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Product } from "../../models/product.model";
import { TResult } from "@app/core/models/result.model";

@Injectable({ providedIn:'root' })
export class ProductsApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient) {}

  getAllProducts(): Promise<TResult<Product[]>> {
    // let allProducts = products;
    // return lastValueFrom(of(allProducts));
    return lastValueFrom(
        this.http.get<TResult<Product[]>>(`${this.apiUrl}/Products`)
    );
  }

//   getProductById(productId: number): Observable<Product> {
//     return this.http.get<FileItemResponseModel>(
//       `${httpAppConfig.apiEndpoint}/FileItems/withPath/${productId}`,
//     );
//   }
}
