// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@features/identity/services/auth.service';

/**
 * Main navigation and account actions of the header (Handoff §3.1, state Anonymous). `bar` is the
 * row of the full header (from 600 px), `drawer` the content of the compact menu.
 *
 * Signed in (until the SignedIn state comes with step 3): „Konto“, „Einstellungen“ and
 * „Abmelden“ replace „Anmelden“ (owner decision O-S2-4, S2-15).
 *
 * @spec docs/design_handoff_procureaccess_v1.md §3.1, §7
 */
@Component({
  selector: 'pa-main-nav',
  imports: [MatButtonModule, RouterLink, RouterLinkActive, TranslatePipe],
  templateUrl: './main-nav.html',
  styleUrl: './main-nav.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { '[class]': '"pa-main-nav pa-main-nav--" + layout()' },
})
export class MainNav {
  readonly layout = input.required<'bar' | 'drawer'>();
  /** A link or action was chosen: the compact menu closes. */
  readonly navigated = output();

  private readonly store = inject(ProcureAccessStore);
  private readonly authService = inject(AuthService);

  private readonly navigationEnd = toSignal(
    inject(Router).events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  // The token lives in localStorage (legacy until step 3), which no signal tracks. It is read
  // again when the user changes (login, „Abmelden“) and after every navigation: the other legacy
  // paths that remove it (account deletion, failed token refresh) navigate to /home.
  protected readonly signedIn = computed(() => {
    this.store.user();
    this.navigationEnd();
    return this.store.isAuthenticated();
  });

  protected login(): void {
    this.navigated.emit();
    this.authService.showLogin();
  }

  protected logout(): void {
    this.navigated.emit();
    this.authService.logout();
    this.store.setUser(null);
    this.store.setProposals([]);
    void this.store.reloadUICustomization();
  }
}
