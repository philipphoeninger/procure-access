// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatCheckboxChange, MatCheckboxModule } from '@angular/material/checkbox';
import { TranslatePipe } from '@ngx-translate/core';

import { ReasonLine } from '../../state/selection-rules';

/** A link after the hint line of a test type (`testType.link`, opens in a new tab). */
export interface OptionLink {
  url: string;
  domain: string;
}

/**
 * One option of the S10 checkbox groups (steps 2–4, Handoff §5.4): the checkbox and the lines
 * below it, each linked to the checkbox with `aria-describedby`.
 *
 * - **Excluded** (`reasons` not empty) or **locked** (Basisfunktionalität): `aria-disabled="true"`
 *   through `disabledInteractive`, never `disabled`, so the option stays focusable and its lines
 *   are read; activating it changes nothing (CFG-02, CFG-03). An excluded option shows only its
 *   reason lines, in place of hint and link (Figma S10 steps 2 and 4).
 * - Otherwise the hint (a UX copy key), the catalog explanation and the link, in this order.
 *
 * The checkbox never changes on its own: a click asks the page through `toggled`, and `checked`
 * shows what the store holds — also when the store refuses the change.
 *
 * @spec docs/SPEC_v2.md R-CON-03 – R-CON-06; docs/design_handoff_procureaccess_v1.md §5.4
 * @invariants CFG-02, CFG-03
 */
@Component({
  selector: 'pa-filter-option',
  imports: [MatCheckboxModule, TranslatePipe],
  templateUrl: './filter-option.html',
  styleUrl: './filter-option.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'pa-filter-option',
    '[class.pa-filter-option--locked]': 'locked()',
  },
})
export class FilterOptionCheckbox {
  /** Unique within the page, e.g. `pa-app-type-web`; the ids of the lines start with it. */
  readonly optionId = input.required<string>();
  readonly name = input.required<string>();
  readonly checked = input(false);
  /** Always selected and never removable (Basisfunktionalität, R-CON-04). */
  readonly locked = input(false);
  /** Why the option is excluded (`criteria.select.excluded.*`); empty = available. */
  readonly reasons = input<ReasonLine[]>([]);
  readonly hintKey = input<string | null>(null);
  /** Catalog explanation, given only while „Erklärungen anzeigen“ is on. */
  readonly explanation = input<string | null>(null);
  readonly link = input<OptionLink | null>(null);

  readonly toggled = output<void>();

  protected readonly unavailable = computed(() => this.locked() || this.reasons().length > 0);
  protected readonly lockedId = computed(() => `${this.optionId()}-locked`);
  protected readonly hintId = computed(() => `${this.optionId()}-hint`);
  protected readonly explanationId = computed(() => `${this.optionId()}-explanation`);

  protected readonly describedBy = computed(() => {
    const reasons = this.reasons();
    const ids =
      reasons.length > 0
        ? reasons.map((_, index) => this.reasonId(index))
        : [
            this.locked() ? this.lockedId() : null,
            this.hintKey() ? this.hintId() : null,
            this.explanation() ? this.explanationId() : null,
          ].filter((id): id is string => id !== null);
    // The checkbox input takes a string only; an empty one names no description.
    return ids.join(' ');
  });

  protected reasonId(index: number): string {
    return `${this.optionId()}-reason-${index}`;
  }

  protected onChange(event: MatCheckboxChange): void {
    // Undo the checkbox's own toggle: `checked` follows the store once it took the change.
    event.source.checked = !event.checked;
    this.toggled.emit();
  }
}
