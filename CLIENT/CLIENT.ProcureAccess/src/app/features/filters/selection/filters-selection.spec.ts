import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FiltersSelection } from './filters-selection';

describe('FiltersSelection', () => {
  let component: FiltersSelection;
  let fixture: ComponentFixture<FiltersSelection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FiltersSelection]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FiltersSelection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
