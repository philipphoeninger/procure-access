// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DeleteAccountDialog } from './delete-account-dialog';

describe('DeleteAccountDialog', () => {
  let component: DeleteAccountDialog;
  let fixture: ComponentFixture<DeleteAccountDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeleteAccountDialog]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DeleteAccountDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
