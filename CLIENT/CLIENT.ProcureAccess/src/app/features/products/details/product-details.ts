import { Component, inject, signal } from '@angular/core';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ActivatedRoute } from '@angular/router';
import { Product } from '../models/product.model';
import { MatDividerModule } from '@angular/material/divider';

@Component({
  selector: 'pa-product-details',
  imports: [
    MatDividerModule
  ],
  templateUrl: './product-details.html',
  styleUrl: './product-details.scss'
})
export class ProductDetails {
  protected store = inject(ProcureAccessStore);
  private route = inject(ActivatedRoute);

  readonly productId = signal<number | null>(null);
  protected product = signal<Product | null>(null);

  constructor(protected snackbarService: SnackbarService) {
    let routeId = this.route.snapshot.paramMap.get('id');
    if (routeId == null) return; //gate
    this.productId.set(+routeId);
  }

  ngOnInit() {
    this.product.set(this.store.getProductById(this.productId()!));
  }
}
