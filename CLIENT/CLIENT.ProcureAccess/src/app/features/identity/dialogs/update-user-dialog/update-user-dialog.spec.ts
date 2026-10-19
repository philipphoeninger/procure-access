// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdateUserDialog } from './update-user-dialog';

describe('UpdateUserDialog', () => {
  let component: UpdateUserDialog;
  let fixture: ComponentFixture<UpdateUserDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdateUserDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(UpdateUserDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
