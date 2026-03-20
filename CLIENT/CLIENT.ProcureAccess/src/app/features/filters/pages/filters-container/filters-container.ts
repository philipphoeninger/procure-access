import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { FiltersSelection } from '../../selection/filters-selection';

@Component({
  selector: 'pa-filters-container',
  imports: [
    FiltersSelection
  ],
  templateUrl: './filters-container.html',
  styleUrl: './filters-container.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FiltersContainer {
    selectedFilterTypeIdsOut = output<number[]>();
}
