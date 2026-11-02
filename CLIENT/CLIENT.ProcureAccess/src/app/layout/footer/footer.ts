// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

/** The public repository of the project (CLAUDE §1). */
export const SOURCE_CODE_URL = 'https://github.com/philipphoeninger/procure-access';

/**
 * Footer (R-SHL-06, Handoff §5.3: not designed, a link list on `surface-container`).
 *
 * Only links with a target so far (owner, S2-15): „Über ProcureAccess“ (no page), „Projekt
 * WERTE.IT“ (no URL) and „Kriterium vorschlagen“ (R-SUG-01, step 2) follow when their target exists.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §5.3
 */
@Component({
  selector: 'pa-footer',
  imports: [RouterLink, TranslatePipe],
  templateUrl: './footer.html',
  styleUrl: './footer.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Footer {
  protected readonly sourceCodeUrl = SOURCE_CODE_URL;
}
