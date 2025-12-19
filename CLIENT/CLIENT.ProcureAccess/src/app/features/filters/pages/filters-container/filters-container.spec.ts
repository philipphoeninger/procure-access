import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FiltersContainer } from './filters-container';

describe('FiltersContainer', () => {
  let component: FiltersContainer;
  let fixture: ComponentFixture<FiltersContainer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FiltersContainer]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FiltersContainer);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
