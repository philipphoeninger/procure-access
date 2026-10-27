// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Component, computed, inject, signal, ChangeDetectionStrategy } from '@angular/core';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { ActivatedRoute } from '@angular/router';
import { Product } from '../models/product.model';
import { MatDividerModule } from '@angular/material/divider';
import { TranslatePipe } from '@ngx-translate/core';

@Component({
  selector: 'pa-product-details',
  imports: [
    MatDividerModule,
    TranslatePipe
  ],
  templateUrl: './product-details.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './product-details.scss'
})
export class ProductDetails {
  protected store = inject(ProcureAccessStore);
  private route = inject(ActivatedRoute);

  readonly productId = signal<number | null>(null);
  // Products load asynchronously; derive the product from the store.
  protected product = computed<Product | null>(() =>
    this.store.getProductById(this.productId()!)
  );

  constructor(protected snackbarService: SnackbarService) {
    let routeId = this.route.snapshot.paramMap.get('id');
    if (routeId == null) return; //gate
    this.productId.set(+routeId);
  }
}
