import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { httpAppConfig } from "@app/app.config";
import { lastValueFrom, Observable, of } from "rxjs";
import { Product } from "../../models/product.model";

@Injectable({ providedIn:'root' })
export class ProductsApiService {
  constructor(private http: HttpClient) {}

  getAllProducts(): Promise<Product[]> {
    // let allProducts = products;
    // return lastValueFrom(of(allProducts));
    return lastValueFrom(
        this.http.get<Product[]>(`${httpAppConfig.apiEndpoint}/Products`)
    );
  }

//   getProductById(productId: number): Observable<Product> {
//     return this.http.get<FileItemResponseModel>(
//       `${httpAppConfig.apiEndpoint}/FileItems/withPath/${productId}`,
//     );
//   }
}
