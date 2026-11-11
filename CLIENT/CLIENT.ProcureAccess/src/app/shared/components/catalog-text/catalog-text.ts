// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  Injector,
  input,
  signal,
} from '@angular/core';
import { TranslatePipe } from '@ngx-translate/core';

import {
  CatalogBlock,
  CatalogListItem,
  limitTopLevelItems,
  parseCatalogText,
} from './catalog-markdown';

interface ShownBlock {
  block: CatalogBlock;
  /** Index of the list's first item among all top-level items of the text; 0 for a paragraph. */
  firstItem: number;
}

/**
 * A catalog text (BIT inklusiv, verbatim) rendered from its Markdown lines as paragraphs, lists,
 * bold and http(s) links (owner decisions O-S4-30 (a), O-S4-36 (a); `catalog-markdown.ts`). Built
 * from Angular nodes only, never `innerHTML`. A link opens in a new tab and says so
 * (`common.newTab`).
 *
 * `maxItems` shows only the first top-level list items (the examples of S11, O-S4-37 (a));
 * `focusItem(index)` moves the focus to a top-level item after the next render, so a „show all“
 * button can hand the focus to the first item it revealed.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.5; docs/sprints/sprint_03/S3-15_release_data_format.md P6
 */
@Component({
  selector: 'pa-catalog-text',
  imports: [NgTemplateOutlet, TranslatePipe],
  templateUrl: './catalog-text.html',
  styleUrl: './catalog-text.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-catalog-text' },
})
export class CatalogText {
  readonly text = input.required<string>();
  /** Null shows every item. */
  readonly maxItems = input<number | null>(null);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly injector = inject(Injector);
  protected readonly focusableItem = signal<number | null>(null);

  private readonly blocks = computed(() => parseCatalogText(this.text()));

  protected readonly shown = computed<ShownBlock[]>(() => {
    const max = this.maxItems();
    const blocks = max === null ? this.blocks() : limitTopLevelItems(this.blocks(), max);
    let next = 0;
    return blocks.map((block) => {
      const firstItem = next;
      next += block.kind === 'list' ? block.items.length : 0;
      return { block, firstItem };
    });
  });

  /** Focus the top-level item at `index` (0-based over the whole text) once it is rendered. */
  focusItem(index: number): void {
    this.focusableItem.set(index);
    afterNextRender(
      () =>
        this.host.nativeElement.querySelector<HTMLElement>(`[data-pa-item="${index}"]`)?.focus(),
      { injector: this.injector },
    );
  }

  protected itemsOf(block: CatalogBlock): CatalogListItem[] {
    return block.kind === 'list' ? block.items : [];
  }
}
