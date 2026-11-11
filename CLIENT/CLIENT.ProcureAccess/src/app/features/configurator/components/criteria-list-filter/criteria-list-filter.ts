// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, ElementRef, input, output, viewChild } from '@angular/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { TranslatePipe } from '@ngx-translate/core';

import { PriorityLevel } from '../../models/criteria';

/** The levels in the order K5 names them (Handoff §4.5). */
const LEVELS: readonly PriorityLevel[] = ['A', 'B', 'Info'];

/**
 * The filter row of S11 (R-LST-04, R-LST-06; Handoff §5.1 S11): a group `criteria.list.filter.label`
 * with the search field, the priority levels as a checkbox group „Priorität“ (none checked = every
 * level, owner decision O-S4-44 (a)) and „Nur aufgenommene Kriterien anzeigen“.
 *
 * It only shows the state it gets and emits each change; the S11 page hands them to the store,
 * which applies the search 300 ms after the last keystroke. Part of the S11 frames, not a Figma
 * component (the Figma filter row has no priority filter yet).
 *
 * @spec docs/SPEC_v2.md R-LST-04, R-LST-06; docs/design_handoff_procureaccess_v1.md §5.1 (S11)
 */
@Component({
  selector: 'pa-criteria-list-filter',
  imports: [MatCheckboxModule, MatFormFieldModule, MatInputModule, TranslatePipe],
  templateUrl: './criteria-list-filter.html',
  styleUrl: './criteria-list-filter.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaListFilterRow {
  /** The search field as typed. */
  readonly searchText = input.required<string>();
  /** The levels shown; empty = every level. */
  readonly levels = input.required<readonly PriorityLevel[]>();
  readonly onlyIncluded = input.required<boolean>();

  readonly searchTextChange = output<string>();
  readonly levelsChange = output<PriorityLevel[]>();
  readonly onlyIncludedChange = output<boolean>();

  protected readonly allLevels = LEVELS;

  private readonly searchField = viewChild.required<ElementRef<HTMLInputElement>>('search');

  /** After „Filter zurücksetzen“ the focus goes here, as the button is gone. */
  focusSearch(): void {
    this.searchField().nativeElement.focus();
  }

  protected levelKey(level: PriorityLevel): string {
    return `criteria.priority.${level}`;
  }

  protected toggleLevel(level: PriorityLevel, checked: boolean): void {
    const others = this.levels().filter((l) => l !== level);
    this.levelsChange.emit(LEVELS.filter((l) => (l === level ? checked : others.includes(l))));
  }
}
