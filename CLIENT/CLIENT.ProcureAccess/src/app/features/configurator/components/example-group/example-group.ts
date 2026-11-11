// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  input,
  signal,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { TranslatePipe } from '@ngx-translate/core';

import {
  countTopLevelItems,
  parseCatalogText,
} from '@app/shared/components/catalog-text/catalog-markdown';
import { CatalogText } from '@app/shared/components/catalog-text/catalog-text';

/** Examples shown before „Alle {{count}} Beispiele anzeigen“ (Handoff §5.5). */
export const EXAMPLES_SHOWN = 3;

/**
 * One group of examples in „Woran erkennt man es?“ — „So ist es gut“ or „So ist es nicht gut“ as
 * H4, then the catalog text. A group with more than three examples (top-level list items) shows
 * the first three and `criteria.item.detail.examples.more` with its own count; the notes after the
 * list stay visible (owner decision O-S4-37 (a)). The button names the group and the criterion
 * through `aria-describedby`; after it the focus goes to the first revealed example and the button
 * is gone.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.5
 */
@Component({
  selector: 'pa-example-group',
  imports: [CatalogText, MatButtonModule, TranslatePipe],
  templateUrl: './example-group.html',
  styleUrl: './example-group.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-example-group' },
})
export class ExampleGroup {
  /** `criteria.item.detail.examples.positive` or `.negative`. */
  readonly titleKey = input.required<string>();
  readonly text = input.required<string>();
  /** Unique id of the H4. */
  readonly headingId = input.required<string>();
  /** Id of the criterion title, so the button says which criterion it belongs to. */
  readonly criterionTitleId = input.required<string>();

  private readonly catalogText = viewChild.required(CatalogText);
  protected readonly EXAMPLES_SHOWN = EXAMPLES_SHOWN;
  protected readonly showAll = signal(false);

  protected readonly count = computed(() => countTopLevelItems(parseCatalogText(this.text())));
  protected readonly limited = computed(() => !this.showAll() && this.count() > EXAMPLES_SHOWN);
  protected readonly describedBy = computed(() => `${this.headingId()} ${this.criterionTitleId()}`);

  protected showAllExamples(): void {
    this.showAll.set(true);
    this.catalogText().focusItem(EXAMPLES_SHOWN);
  }
}
