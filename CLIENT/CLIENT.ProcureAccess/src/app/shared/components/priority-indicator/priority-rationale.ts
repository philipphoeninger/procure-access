// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { PriorityRuleText } from './priority-level';

/**
 * Why a criterion has its priority (CAT-02, PRD §7 „Transparenz“): the rationale of the project
 * team, the rule that matched (`criteria.priority.rule.*`) and the source line „Grundlage:
 * Empfehlung des Projektteams WERTE.IT …“. All three are required and always shown together; the
 * source is not optional (UX copy G8). Part of K5 (Handoff §4.5): the criteria list S11 shows it as
 * the first section of a card's detail panel, a later K5 toggle in its own panel.
 *
 * @spec docs/SPEC_v2.md R-LST-02; docs/design_handoff_procureaccess_v1.md §4.5, §5.5
 * @invariants CAT-02
 */
@Component({
  selector: 'pa-priority-rationale',
  imports: [TranslatePipe],
  templateUrl: './priority-rationale.html',
  styleUrl: './priority-rationale.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PriorityRationale {
  /** `RationaleDe` of the matched priority rule, catalog content shown verbatim. */
  readonly rationale = input.required<string>();
  readonly rule = input.required<PriorityRuleText>();
}
