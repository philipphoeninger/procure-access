// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductProposal } from './product-proposal';

describe('ProductProposal', () => {
  let component: ProductProposal;
  let fixture: ComponentFixture<ProductProposal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductProposal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductProposal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
