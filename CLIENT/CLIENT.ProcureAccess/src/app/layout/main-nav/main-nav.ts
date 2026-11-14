// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@features/identity/services/auth.service';
import { CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '@app/features/configurator/guards/configurator-guards';

/**
 * Main navigation and account actions of the header (Handoff §3.1, state Anonymous). `bar` is the
 * row of the full header (from 600 px), `drawer` the content of the compact menu.
 *
 * Signed in (until the SignedIn state comes with step 3): „Konto“, „Einstellungen“ and
 * „Abmelden“ replace „Anmelden“ (owner decision O-S2-4, S2-15).
 *
 * „Kriterien“ links S10; without an active catalog release its guard opens the legacy filter
 * instead (O-S4-3), so the item counts as current on both.
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
  private readonly router = inject(Router);
  private readonly pageTitle = inject(PageTitleStrategy);

  private readonly navigationEnd = toSignal(
    this.router.events.pipe(filter((event) => event instanceof NavigationEnd)),
  );

  protected readonly criteriaPath = CONFIGURATOR_PATH;

  /** S10, S11 or the legacy filter that stands in for S10 (O-S4-3). */
  protected readonly criteriaCurrent = computed(() => {
    this.navigationEnd();
    const path = this.router.url.split(/[?#(]/)[0];
    return [CONFIGURATOR_PATH, LEGACY_FILTER_PATH].some((base) => path === base || path.startsWith(`${base}/`));
  });

  /** From the session of `withIdentity()` (S5-08): follows sign-in, sign-out and account deletion. */
  protected readonly signedIn = this.store.signedIn;

  /** S01 over the current page, which stays (R-AUTH-03). */
  protected login(): void {
    this.navigated.emit();
    void this.authService.showLogin();
  }

  /**
   * Ends the session and clears every in-memory state of the user (R-AUTH-08), then shows the
   * start page with the focus on its H1 — also when it was already open, where the router sees no
   * page change.
   */
  protected async logout(): Promise<void> {
    this.navigated.emit();
    // The session is forgotten here also when the request fails; the error interceptor shows that.
    await this.store.endSession().catch(() => undefined);
    await this.router.navigateByUrl('/');
    this.pageTitle.focusPageHeading();
  }
}
