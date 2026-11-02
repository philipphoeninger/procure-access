// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MainNav } from '@layout/main-nav/main-nav';
import { injectWideLayout } from '@layout/shell/wide-layout';

/**
 * pa-app-header · Kopfzeile — one component with two layouts (Handoff §3.1): below 600 px the
 * compact header (app name + „Menü“, Figma `pa-app-header-compact` 2039:2131), from 600 px the
 * full header with the main navigation. State Anonymous only; SignedIn and SignedInNoOrg come
 * with step 3.
 *
 * The compact menu itself is the shell's `mat-sidenav` (it must be a child of the sidenav
 * container), so this component only reports the toggle and shows its state.
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2017-650
 * @spec docs/design_handoff_procureaccess_v1.md §3.1, §2.2
 */
@Component({
  selector: 'pa-header',
  imports: [MatButtonModule, RouterLink, TranslatePipe, MainNav],
  templateUrl: './header.html',
  styleUrl: './header.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Header {
  /** The compact menu is open (`aria-expanded` of „Menü“). */
  readonly menuOpen = input(false);
  /** „Menü“ was pressed. */
  readonly menuToggle = output();

  protected readonly wide = injectWideLayout();
}
