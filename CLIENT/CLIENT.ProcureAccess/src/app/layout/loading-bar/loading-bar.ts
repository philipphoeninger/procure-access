// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, effect, inject, untracked } from '@angular/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TranslatePipe } from '@ngx-translate/core';
import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';

/**
 * Loading state of the shell (R-SHL-09, Handoff §6.2): an indeterminate `mat-progress-bar` at the
 * top of the content column while the shared `withLoading()` counter is above zero, and one
 * `common.loading` announcement each time loading starts. No skeleton screens. `common.loaded`
 * comes with the first screen whose load has a count.
 *
 * @spec docs/SPEC_v2.md R-SHL-09; docs/design_handoff_procureaccess_v1.md §6.2
 */
@Component({
  selector: 'pa-loading-bar',
  imports: [MatProgressBarModule, TranslatePipe],
  templateUrl: './loading-bar.html',
  styleUrl: './loading-bar.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoadingBar {
  protected readonly loading = inject(ProcureAccessStore).isLoading;
  private readonly announcer = inject(A11yAnnouncerService);

  constructor() {
    // `isLoading` is computed, so it changes only when loading starts or ends, not per request.
    effect(() => {
      if (this.loading()) untracked(() => void this.announcer.announce('common.loading'));
    });
  }
}
