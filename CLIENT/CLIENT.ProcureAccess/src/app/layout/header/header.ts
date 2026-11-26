// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, ElementRef, input, output, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { MainNav } from '@layout/main-nav/main-nav';
import { injectWideLayout } from '@layout/shell/wide-layout';

/**
 * pa-app-header · Kopfzeile — one component with two layouts (Handoff §3.1): below 600 px the
 * compact header (app name + „Menü“, Figma `pa-app-header-compact` 2039:2131), from 600 px the
 * full header with the main navigation. The states Anonymous, SignedIn and SignedInNoOrg
 * (R-SHL-02, S5-18) and the organisation switch (R-SHL-03) live in `pa-main-nav`, which both
 * layouts show.
 *
 * The compact menu itself is the shell's `mat-sidenav` (it must be a child of the sidenav
 * container), so this component reports the toggle, shows its state and takes the focus back
 * when the shell closes the menu ({@link focusMenuButton}).
 *
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2017-650
 * @figma https://www.figma.com/design/rPeSyK8NNe3d8AnUEXW9mt/ProcureAccess?node-id=2039-2131 (compact)
 * @spec docs/design_handoff_procureaccess_v1.md §3.1, §2.2; docs/SPEC_v2.md R-SHL-02, R-SHL-03
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

  private readonly menuButton = viewChild<string, ElementRef<HTMLButtonElement>>('menuButton', { read: ElementRef });

  /** Moves the focus to „Menü“ (compact layout only; nothing happens in the full header). */
  focusMenuButton(): void {
    this.menuButton()?.nativeElement.focus();
  }
}
