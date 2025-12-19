import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {MatButtonModule} from '@angular/material/button';
import {provideNativeDateAdapter} from '@angular/material/core';
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatIconModule} from '@angular/material/icon';
import {MatInputModule} from '@angular/material/input';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { CriteriaApiService } from '../services/api/criteria-api.service';

@Component({
  selector: 'pa-criteria-list',
  templateUrl: 'criteria-list.html',
  styleUrl: 'criteria-list.scss',
  providers: [provideNativeDateAdapter()],
  imports: [
    MatExpansionModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatDatepickerModule,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaList {
  protected store = inject(ProcureAccessStore);

  step = signal(0);

  constructor(
    protected criteriaApiService: CriteriaApiService
  ) {}

  ngOnInit() {}

  setStep(index: number) {
    this.step.set(index);
  }

  nextStep() {
    this.step.update(i => i + 1);
  }

  prevStep() {
    this.step.update(i => i - 1);
  }
}
