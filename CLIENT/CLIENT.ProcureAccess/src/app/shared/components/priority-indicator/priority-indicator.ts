// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { PriorityLevel } from './priority-level';

/**
 * K5 pa-priority-indicator · Priorität mit Begründung — the level of a criterion, its weight for B,
 * and, while the user changed it, the recommendation next to it (CAT-08: the recommendation stays
 * visible).
 *
 * Today K5 shows the level only. The menu that changes it („Priorität: {{level}}“, Handoff §4.5)
 * comes with S4-16, so there is no button that opens nothing. The rationale, the matched rule and
 * the source are {@link PriorityRationale}: in the criteria list S11 they are the first section of
 * the card's detail panel (`showRationaleToggle=false`, U28); the „Warum diese Priorität?“ toggle of
 * K5 itself comes with the first screen that shows K5 without such a panel.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2011-203
 * @spec docs/SPEC_v2.md R-LST-02, R-LST-03; docs/design_handoff_procureaccess_v1.md §4.5
 * @invariants CAT-02, CAT-08
 */
@Component({
  selector: 'pa-priority-indicator',
  imports: [TranslatePipe],
  templateUrl: './priority-indicator.html',
  styleUrl: './priority-indicator.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-priority-indicator' },
})
export class PriorityIndicator {
  /** The level that applies now: the user's, else the recommendation. */
  readonly level = input.required<PriorityLevel>();
  /** The recommendation of the project team; shown as „Empfohlen war“ when it differs from `level`. */
  readonly recommendedLevel = input.required<PriorityLevel>();
  /** Shown only for B. */
  readonly weight = input<number | null>(null);

  protected readonly levelKey = computed(() => levelKey(this.level()));
  protected readonly recommendedKey = computed(() => levelKey(this.recommendedLevel()));
  protected readonly shownWeight = computed(() => (this.level() === 'B' ? this.weight() : null));
  protected readonly overridden = computed(() => this.level() !== this.recommendedLevel());
}

function levelKey(level: PriorityLevel): string {
  return `criteria.priority.${level}`;
}
