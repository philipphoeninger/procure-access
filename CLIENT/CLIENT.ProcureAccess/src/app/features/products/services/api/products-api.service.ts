import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { API_URL } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Product } from "../../models/product.model";

@Injectable({ providedIn:'root' })
export class ProductsApiService {
  constructor(
    @Inject(API_URL) private apiUrl: string,
    private http: HttpClient) {}

  getAllProducts(): Promise<Product[]> {
    // let allProducts = products;
    // return lastValueFrom(of(allProducts));
    return lastValueFrom(
        this.http.get<Product[]>(`${this.apiUrl}/Products`)
    );
  }

//   getProductById(productId: number): Observable<Product> {
//     return this.http.get<FileItemResponseModel>(
//       `${httpAppConfig.apiEndpoint}/FileItems/withPath/${productId}`,
//     );
//   }
}
