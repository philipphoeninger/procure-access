// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CriterionProposal } from './criterion-proposal';

describe('CriterionProposal', () => {
  let component: CriterionProposal;
  let fixture: ComponentFixture<CriterionProposal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CriterionProposal]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CriterionProposal);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
