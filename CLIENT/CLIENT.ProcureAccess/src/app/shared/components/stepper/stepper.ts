// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { CdkStepper } from '@angular/cdk/stepper';
import { NgTemplateOutlet } from '@angular/common';
import {
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  Injector,
  input,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { injectWideLayout } from '@app/layout/shell/wide-layout';

/**
 * pa-stepper — a linear stepper whose step headers only show progress (SPEC R-CON-01: display
 * only, `aria-current="step"`). Built on CDK `CdkStepper` instead of `mat-stepper`, whose
 * horizontal headers are clickable tabs (ARCHITECTURE C117, owner decision S4-10 A (a)).
 *
 * - From 600 px: an ordered list of the step names; the current one carries `aria-current="step"`,
 *   finished ones a check icon and the hidden suffix `common.stepper.done`.
 * - Below 600 px: the text line `stepLabelKey` („Schritt 1 von 4: Verwendungszweck“) instead
 *   (Handoff §2.2, S10-M).
 * - A step change moves the focus to the first heading of the new step and announces
 *   `stepLabelKey`. The page moves on with `next()` / `previous()` after its own check; steps
 *   carry `[completed]`, so `linear` refuses to skip an unfinished one.
 *
 * Use with `<cdk-step [label]="…" [completed]="…">` children. Reused by S35 and S55 later.
 *
 * @spec docs/SPEC_v2.md R-CON-01; docs/design_handoff_procureaccess_v1.md §2.2, §5.1 (S10)
 */
@Component({
  selector: 'pa-stepper',
  imports: [MatIconModule, NgTemplateOutlet, TranslatePipe],
  templateUrl: './stepper.html',
  styleUrl: './stepper.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: CdkStepper, useExisting: Stepper }],
})
export class Stepper extends CdkStepper {
  /** UX copy key with `{{current}}`, `{{total}}` and `{{name}}`, e.g. `criteria.select.stepLabel`. */
  readonly stepLabelKey = input.required<string>();

  protected readonly wide = injectWideLayout();
  private readonly content = viewChild.required<ElementRef<HTMLElement>>('content');
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly injector = inject(Injector);

  /** A method, not `computed`: the step labels are inputs of `CdkStep`, not signals. */
  protected stepLabelParams() {
    const index = this.selectedIndex;
    return { current: index + 1, total: this.steps.length, name: this.steps.get(index)?.label ?? '' };
  }

  constructor() {
    super();
    this.selectionChange.pipe(takeUntilDestroyed()).subscribe(() => {
      afterNextRender(
        () => {
          this.focusStepHeading();
          void this.announcer.announce(this.stepLabelKey(), this.stepLabelParams());
        },
        { injector: this.injector },
      );
    });
  }

  private focusStepHeading(): void {
    const heading = this.content().nativeElement.querySelector<HTMLElement>('h2, h3');
    if (!heading) return;
    if (!heading.hasAttribute('tabindex')) heading.setAttribute('tabindex', '-1');
    heading.focus();
  }
}
