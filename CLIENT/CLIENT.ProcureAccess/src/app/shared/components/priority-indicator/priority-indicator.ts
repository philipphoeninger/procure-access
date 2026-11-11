// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import {
  ChangeDetectionStrategy,
  Component,
  computed,
  ElementRef,
  inject,
  input,
  model,
  output,
  viewChild,
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';

import { PRIORITY_LEVELS, PriorityLevel } from './priority-level';

/** The range of a weight the user may choose (R-LST-03: 1–10, a 🔧 value the caller reads from configuration). */
export interface PriorityWeightRange {
  min: number;
  max: number;
}

/** What the override line says, or null while the recommendation applies unchanged. */
interface OverrideLine {
  key: string;
  /** Key of the recommended level, translated in the template. */
  recommendedKey: string;
  /** The recommended weight, for `criteria.priority.overridden.weight` only. */
  weight: number | null;
}

let nextId = 0;

/**
 * K5 pa-priority-indicator · Priorität mit Begründung — the level of a criterion with the menu that
 * changes it, its weight for B, and, while the user changed it, the recommendation next to it with
 * „Empfehlung wiederherstellen“ (R-LST-03, CAT-08: the recommendation stays visible).
 *
 * - Level button (`criteria.priority.button`, described by the criterion title) opens a `mat-menu`
 *   (`criteria.priority.changeLabel`) with one `menuitemradio` per level. After a choice the menu
 *   returns the focus to the button and `criteria.priority.changed` is announced.
 * - Weight: with a `weightRange` (the criteria list S11) a `mat-select` `criteria.priority.weightLabel`
 *   while B applies (O-S4-40 (a)); without one the weight is part of the button, as in Figma.
 * - Override line: `criteria.priority.overridden` when the level differs from the recommendation,
 *   `criteria.priority.overridden.weight` when only the weight of a recommended B does (O-S4-41 (a)).
 *   „Empfehlung wiederherstellen“ emits `restoreRecommendation`, puts the focus on the level
 *   button (the reset button goes away) and announces the recommended level.
 *
 * The rationale, the matched rule and the source are {@link PriorityRationale}: in the criteria
 * list S11 they are the first section of the card's detail panel (`showRationaleToggle=false`, U28);
 * the „Warum diese Priorität?“ toggle of K5 itself comes with the first screen that shows K5 without
 * such a panel.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2011-203
 * @spec docs/SPEC_v2.md R-LST-02, R-LST-03; docs/design_handoff_procureaccess_v1.md §4.5
 * @invariants CAT-02, CAT-08
 */
@Component({
  selector: 'pa-priority-indicator',
  imports: [MatButtonModule, MatFormFieldModule, MatIconModule, MatMenuModule, MatSelectModule, TranslatePipe],
  templateUrl: './priority-indicator.html',
  styleUrl: './priority-indicator.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'pa-priority-indicator' },
})
export class PriorityIndicator {
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly translate = inject(TranslateService);

  /** The level that applies now: the user's, else the recommendation. Set by the menu. */
  readonly level = model.required<PriorityLevel>();
  /** The recommendation of the project team; shown as „Empfohlen war“ when it differs. */
  readonly recommendedLevel = input.required<PriorityLevel>();
  /** The weight that applies now; used only for B. Set by the weight field. */
  readonly weight = model<number | null>(null);
  /** The recommended weight when the recommendation is B. */
  readonly recommendedWeight = input<number | null>(null);
  /** Id of the criterion heading; describes the level button, the weight field and the reset button. */
  readonly criterionTitleId = input<string | null>(null);
  /** With a range the weight of B can be chosen; without one it is shown only. */
  readonly weightRange = input<PriorityWeightRange | null>(null);
  /** „Empfehlung wiederherstellen“ (Handoff §4.5 `(reset)`; `reset` is a DOM event name). */
  readonly restoreRecommendation = output<void>();

  protected readonly levels = PRIORITY_LEVELS;
  protected readonly weightFieldId = `pa-priority-weight-${nextId++}`;
  private readonly levelButton = viewChild.required<string, ElementRef<HTMLElement>>('levelButton', {
    read: ElementRef,
  });

  protected readonly levelKey = computed(() => levelKey(this.level()));
  /** The weight field shows while B applies and a range is given. */
  protected readonly weightEditable = computed(() => this.level() === 'B' && this.weightRange() !== null);
  /** The weight inside the level button: B with a weight and no field. */
  protected readonly buttonWeight = computed(() =>
    this.level() === 'B' && !this.weightEditable() ? this.weight() : null,
  );
  protected readonly weightOptions = computed(() => {
    const range = this.weightRange();
    if (range === null) {
      return [];
    }
    return Array.from({ length: range.max - range.min + 1 }, (_, i) => range.min + i);
  });

  protected readonly override = computed<OverrideLine | null>(() => {
    const recommended = this.recommendedLevel();
    const recommendedKey = levelKey(recommended);
    if (this.level() !== recommended) {
      return { key: 'criteria.priority.overridden', recommendedKey, weight: null };
    }
    if (recommended !== 'B' || this.weight() === this.recommendedWeight()) {
      return null;
    }
    // Only the weight of a recommended B differs (O-S4-41 (a)); a B without weight names none.
    const weight = this.recommendedWeight();
    return weight === null
      ? { key: 'criteria.priority.overridden', recommendedKey, weight: null }
      : { key: 'criteria.priority.overridden.weight', recommendedKey, weight };
  });

  protected choose(level: PriorityLevel): void {
    if (level === this.level()) {
      return;
    }
    this.level.set(level);
    void this.announcer.announce('criteria.priority.changed', { level: this.levelText(level) });
  }

  protected chooseWeight(weight: number): void {
    this.weight.set(weight);
  }

  protected restore(): void {
    this.restoreRecommendation.emit();
    this.levelButton().nativeElement.focus();
    void this.announcer.announce('criteria.priority.changed', {
      level: this.levelText(this.recommendedLevel()),
    });
  }

  protected levelKeyOf(level: PriorityLevel): string {
    return levelKey(level);
  }

  private levelText(level: PriorityLevel): string {
    return this.translate.instant(levelKey(level)) as string;
  }
}

function levelKey(level: PriorityLevel): string {
  return `criteria.priority.${level}`;
}
