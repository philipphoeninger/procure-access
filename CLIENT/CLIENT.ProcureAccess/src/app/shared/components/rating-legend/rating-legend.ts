// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, input, model } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TranslatePipe } from '@ngx-translate/core';

/** The rating scale of BIT inklusiv as `assessment.value.*` names it, in scale order (UX copy U21, U24). */
export const ASSESSMENT_VALUES = [
  'fulfilled',
  'slightRestriction',
  'restriction',
  'barrier',
  'blockade',
  'notApplicable',
  'unknown',
] as const;

export type AssessmentValue = (typeof ASSESSMENT_VALUES)[number];

let nextId = 0;

/**
 * pa-rating-legend · Erklärung der Bewertungsstufen — a disclosure „Was bedeuten die
 * Bewertungsstufen?“ with one explanation per rating level and the footnote (UX copy U24).
 *
 * - Text button `assessment.legend.show|hide` with `aria-expanded` and `aria-controls`; the panel
 *   follows it directly in the DOM and stays there while hidden. No tooltip, no dialog.
 * - Panel: a `<dl>` with the level (`assessment.value.*`) as term and `assessment.value.*.explain`
 *   as its explanation, then `assessment.legend.footnote`.
 * - Once per page, never inside every card (Handoff §4.11). On the criteria list S11, a saved list
 *   and the shared list S16 it stands above the list, collapsed, with the levels the criteria of the
 *   list allow (owner decisions #279 (b), O-S5-31-1, O-S5-31-2).
 *
 * The levels are always shown in scale order, whatever order `values` has.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2099-760
 * @spec docs/SPEC_v2.md R-LST-01; docs/design_handoff_procureaccess_v1.md §4.11
 * @invariants A11Y-01
 */
@Component({
  selector: 'pa-rating-legend',
  imports: [MatButtonModule, TranslatePipe],
  templateUrl: './rating-legend.html',
  styleUrl: './rating-legend.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-rating-legend' },
})
export class RatingLegend {
  /** Whether the explanations are shown; collapsed by default. */
  readonly expanded = model(false);
  /** The levels to explain; all seven when not given. */
  readonly values = input<readonly AssessmentValue[]>(ASSESSMENT_VALUES);

  protected readonly panelId = `pa-rating-legend-${nextId++}`;
  protected readonly shown = computed(() => {
    const values = this.values();
    return ASSESSMENT_VALUES.filter((value) => values.includes(value));
  });

  protected toggle(): void {
    this.expanded.update((expanded) => !expanded);
  }
}
