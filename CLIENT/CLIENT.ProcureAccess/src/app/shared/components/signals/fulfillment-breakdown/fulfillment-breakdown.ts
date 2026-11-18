// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { ASSESSMENT_VALUES } from '@app/shared/components/rating-legend/rating-legend';
import { TranslatePluralPipe } from '@app/shared/pipes/translate-plural.pipe';

/**
 * The seven counts of the workspace scope, one per level of the BIT inklusiv scale and
 * „Nicht eingeschätzt“ (SIG-13: never `notCovered`, the assessment is pinned to the list's
 * release). No total, no share, no grouping: the type has nothing to build one from (SIG-08).
 */
export interface WorkspaceFulfillmentCounts {
  fulfilled: number;
  slightRestriction: number;
  restriction: number;
  barrier: number;
  blockade: number;
  notApplicable: number;
  unknown: number;
}

/**
 * The scopes K1 is built for. Only `workspace` (S34 Überblick, S6-16); `configuration` and `public`
 * with their eighth row `notCovered` join with the directory in step 6 (Handoff §4.1).
 */
export type FulfillmentScope = 'workspace';

/**
 * K1 · pa-fulfillment-breakdown · Abgleich mit Kriterien — all fulfilment counts of its scope,
 * never a summary value (Handoff §4.1, ARCHITECTURE §8.4).
 *
 * - Scope „workspace“ (S34 Überblick, Figma `Scope=Workspace, NotCovered=None` `2101:3180`): the
 *   organisation's own assessment of a candidate. Title `candidate.overview.status.breakdownTitle`
 *   with the list name, intro `.breakdownIntro`, then a `<dl>` with exactly seven rows in the fixed
 *   order of the scale (Erfüllt … Nicht eingeschätzt): the level (`assessment.value.*`) as term and
 *   `signals.fulfillment.count` („22 Kriterien“, singular „1 Kriterium“) as value (owner decision
 *   S6-16 D7 (a)). A zero shows as „0 Kriterien“; no row is ever left out or reordered (SIG-08).
 *   No release line and no details button in this scope.
 * - Forbidden by type: sum, percentage, bar, chart, stars, traffic light, a colour per level,
 *   sorting by value, a „konform / nicht konform“ grouping — no input can produce one.
 * - A `<section>` without an accessible name (owner decision S6-16 D5 (a), TESTING V229: axe on
 *   named regions crashes Firefox); its heading still heads it. `headingLevel` fits it under the
 *   heading of its place (H4 under „Einschätzung nach Kriterien“ on S34).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2014-341
 * @spec docs/SPEC_v2.md R-CAN-04; docs/design_handoff_procureaccess_v1.md §4.1
 * @invariants SIG-08, SIG-13
 */
@Component({
  selector: 'pa-fulfillment-breakdown',
  imports: [TranslatePipe, TranslatePluralPipe],
  templateUrl: './fulfillment-breakdown.html',
  styleUrl: './fulfillment-breakdown.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FulfillmentBreakdown {
  readonly counts = input.required<WorkspaceFulfillmentCounts>();
  readonly scope = input<FulfillmentScope>('workspace');
  /** The name of the criteria list the counts belong to (`{{list}}` of the title). */
  readonly listName = input.required<string>();
  /** The level of the title: 3 by default (Handoff §4.1), 4 below an H3. */
  readonly headingLevel = input<3 | 4>(3);

  /** The rows in the fixed order of the scale (SIG-08); never sorted by value. */
  protected readonly rows = ASSESSMENT_VALUES;
}
