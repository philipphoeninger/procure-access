// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

import { SelectionNames } from '../../state/selection-names';

/**
 * The summary above the S11 list (UX copy §5.5, U28; Figma `2101:3160`): the number of requirement
 * criteria for the selection (`criteria.list.summary.count`), then the selection as a description
 * list — use case, app types, product parts and, only with a selection in step 4, test types. The
 * names of a layer are joined by `Intl.ListFormat` in the document language („A, B und C“).
 *
 * Shows only what it gets. Part of the S11 frames, not a Figma component.
 *
 * @spec docs/ux_copy_procureaccess_v22.md §5.5 (`criteria.list.summary.*`); docs/design_handoff_procureaccess_v1.md §5.1 (S11)
 */
@Component({
  selector: 'pa-criteria-list-summary',
  imports: [TranslatePipe, TranslatePluralPipe],
  templateUrl: './criteria-list-summary.html',
  styleUrl: './criteria-list-summary.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CriteriaListSummary {
  /** Requirement criteria of the answer (owner decision O-S4-51 (a)). */
  readonly count = input.required<number>();
  readonly names = input.required<SelectionNames>();

  private readonly list = new Intl.ListFormat(inject(DOCUMENT).documentElement.lang || 'de', {
    type: 'conjunction',
  });

  protected readonly appTypes = computed(() => this.list.format(this.names().appTypes));
  protected readonly productParts = computed(() => this.list.format(this.names().productParts));
  protected readonly testTypes = computed(() => this.list.format(this.names().testTypes));
}
