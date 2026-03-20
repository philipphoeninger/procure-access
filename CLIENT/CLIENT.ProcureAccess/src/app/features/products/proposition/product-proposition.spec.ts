import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProductProposition } from './product-proposition';

describe('ProductProposition', () => {
  let component: ProductProposition;
  let fixture: ComponentFixture<ProductProposition>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductProposition]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductProposition);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
