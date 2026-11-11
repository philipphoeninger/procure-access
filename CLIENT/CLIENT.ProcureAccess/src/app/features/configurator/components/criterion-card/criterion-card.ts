// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT, NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, input, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslatePipe } from '@ngx-translate/core';

import { PriorityIndicator } from '@app/shared/components/priority-indicator/priority-indicator';
import { PriorityRationale } from '@app/shared/components/priority-indicator/priority-rationale';

import { Criterion } from '../../models/criteria';
import { CriterionDecision } from '../../models/criteria-list';
import { CatalogNames, ExternalLink, linkTo } from '../../state/catalog-names';
import { effectiveLevel, effectiveWeight } from '../../state/criteria-list-rules';
import {
  cardAnchor,
  metaLine,
  procedureTestType,
  requirementTitle,
  ruleText,
  sourceTitle,
} from '../../state/criterion-card-rules';
import { detailLayout, tabLabelKey } from '../../state/criterion-detail-rules';
import { ProcedureSections } from '../procedure-sections/procedure-sections';

/** Stands for the link inside `criteria.item.testedBy.meta`; a private-use character no text holds. */
const LINK_MARK = '';

/**
 * One criterion of the criteria list S11 (Handoff §5.5): compact by default, with a detail panel
 * that opens in place. The title is the H2, the sections of the panel are H3 (R-LST-01).
 *
 * - Requirement: title (plain language, else the catalog title, O-S4-32), source title of the
 *   first procedure text, meta line (product part · EN 301 549 clause, U100), K5.
 * - „Geprüft nach …“ (R-LST-07): `criteria.item.testedBy.*` with the test type's link, K5; the
 *   panel adds `criteria.item.testedBy.note` (owner decision O-S4-33 (a)).
 * - The panel starts with „Warum diese Priorität?“: rationale, matched rule and source (R-LST-02,
 *   CAT-02); K5 itself has no toggle here (`showRationaleToggle=false`, U28). The catalog sections
 *   of the procedure texts follow (`pa-procedure-sections`, S4-15b), then „Gilt für“ (`appliesTo`,
 *   O-S4-29) and the source line of the texts. With several procedures the sections whose text is
 *   the same in all stand above a `mat-tab-group` with one tab per procedure (U101); each tab holds
 *   the other sections, „So wird es geprüft“ and its source line. The checkbox and „Notiz
 *   hinzufügen“ follow with S4-16.
 *
 * The toggle keeps the focus and names the criterion through `aria-describedby`; the panel stays in
 * the DOM while hidden, so `aria-controls` always points at it. The title carries the anchor
 * `k-<key>` (the host keeps its own id, which the test library uses to remove a rendered card).
 *
 * @spec docs/SPEC_v2.md R-LST-01, R-LST-02, R-LST-07; docs/design_handoff_procureaccess_v1.md §5.5
 * @invariants CAT-02
 */
@Component({
  selector: 'pa-criterion-card',
  imports: [
    MatButtonModule,
    MatTabsModule,
    NgTemplateOutlet,
    PriorityIndicator,
    PriorityRationale,
    ProcedureSections,
    TranslatePipe,
  ],
  templateUrl: './criterion-card.html',
  styleUrl: './criterion-card.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-criterion-card' },
})
export class CriterionCard {
  readonly criterion = input.required<Criterion>();
  readonly decision = input.required<CriterionDecision>();
  readonly names = input.required<CatalogNames>();

  protected readonly linkMark = LINK_MARK;
  protected readonly expanded = signal(false);

  /** The anchor `k-<key>` is the id of the title, so a link scrolls to the heading. */
  protected readonly titleId = computed(() => cardAnchor(this.criterion().key));
  protected readonly panelId = computed(() => `${this.titleId()}-details`);

  /** The test type of a „Geprüft nach …“ card; null for a requirement. */
  protected readonly testType = computed(() => procedureTestType(this.criterion()));
  protected readonly testTypeKey = computed(() => `testType.${this.testType()}`);
  protected readonly testTypeLink = computed<ExternalLink | null>(() => {
    const testType = this.testType();
    const url = testType === null ? undefined : this.names().testTypeUrls[testType];
    return url === undefined ? null : linkTo(url);
  });

  protected readonly title = computed(() => requirementTitle(this.criterion()));
  protected readonly sourceTitle = computed(() => sourceTitle(this.criterion()));
  protected readonly meta = computed(() => metaLine(this.criterion(), this.names()));

  protected readonly level = computed(() => effectiveLevel(this.criterion(), this.decision()));
  protected readonly recommendedLevel = computed(() => this.criterion().resolvedPriority.level);
  protected readonly weight = computed(() => effectiveWeight(this.criterion(), this.decision()));
  protected readonly rule = computed(() =>
    ruleText(this.criterion().resolvedPriority.matchedRule, this.names()),
  );

  protected readonly layout = computed(() => detailLayout(this.criterion()));
  protected readonly tabLabelKey = tabLabelKey;
  private readonly appTypeList = new Intl.ListFormat(
    inject(DOCUMENT).documentElement.lang || 'de',
    {
      type: 'conjunction',
    },
  );
  /** „Gilt für“: the selected app types the criterion applies to, joined in the document language. */
  protected readonly appliesTo = computed(() => {
    const names = this.criterion().appliesTo.map((appType) => appType.name);
    return names.length === 0 ? null : this.appTypeList.format(names);
  });

  protected toggle(): void {
    this.expanded.update((expanded) => !expanded);
  }

  /** The text of `criteria.item.testedBy.meta` before the link. */
  protected beforeLink(text: string): string {
    return text.split(LINK_MARK)[0] ?? text;
  }

  /** The text of `criteria.item.testedBy.meta` after the link. */
  protected afterLink(text: string): string {
    return text.split(LINK_MARK)[1] ?? '';
  }
}
