// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import { isWebUrl } from '@app/shared/components/catalog-text/catalog-markdown';
import { CatalogText } from '@app/shared/components/catalog-text/catalog-text';

import { ProcedureText } from '../../models/criteria';
import {
  DETAIL_SECTION_TITLE_KEYS,
  DetailSectionKind,
  hasSection,
  RATING_KEYS,
  ratingsOf,
} from '../../state/criterion-detail-rules';
import { ExampleGroup } from '../example-group/example-group';

/**
 * The catalog sections of one procedure text in the detail panel of an S11 card (Handoff §5.5),
 * each an H3 section: „Was wird verlangt?“, „Wann gilt das?“, „Wen betrifft es?“, „Woran erkennt
 * man es?“ (two example groups), „Wie wird bewertet?“ (allowed ratings in scale order) and, with
 * `howTested`, „So wird es geprüft“ with the link to the test step (new tab). The texts are catalog
 * content, shown verbatim through `pa-catalog-text`. `kinds` says which sections to show here —
 * the card puts sections with the same text in every procedure above the tabs.
 *
 * @spec docs/SPEC_v2.md R-LST-01; docs/design_handoff_procureaccess_v1.md §5.5
 */
@Component({
  selector: 'pa-procedure-sections',
  imports: [CatalogText, ExampleGroup, TranslatePipe],
  templateUrl: './procedure-sections.html',
  styleUrl: './procedure-sections.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-procedure-sections' },
})
export class ProcedureSections {
  readonly text = input.required<ProcedureText>();
  readonly kinds = input.required<readonly DetailSectionKind[]>();
  readonly howTested = input(false);
  /** Unique prefix for the ids of the example headings. */
  readonly idPrefix = input.required<string>();
  readonly criterionTitleId = input.required<string>();

  protected readonly titleKeys = DETAIL_SECTION_TITLE_KEYS;
  protected readonly ratingKeys = RATING_KEYS;

  protected readonly shownKinds = computed(() =>
    this.kinds().filter((kind) => hasSection(this.text(), kind)),
  );
  protected readonly ratings = computed(() => ratingsOf(this.text()));
  protected readonly testTypeKey = computed(() => `testType.${this.text().testType}`);
  /** The test step's URL, only an http(s) one. */
  protected readonly stepUrl = computed(() =>
    isWebUrl(this.text().sourceUrl) ? this.text().sourceUrl : null,
  );
}
