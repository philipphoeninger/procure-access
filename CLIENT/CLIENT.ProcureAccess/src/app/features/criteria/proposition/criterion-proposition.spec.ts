import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CriterionProposition } from './criterion-proposition';

describe('CriterionProposition', () => {
  let component: CriterionProposition;
  let fixture: ComponentFixture<CriterionProposition>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CriterionProposition]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CriterionProposition);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
