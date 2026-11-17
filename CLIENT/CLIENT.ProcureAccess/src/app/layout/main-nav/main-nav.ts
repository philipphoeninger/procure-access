// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, computed, inject, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { filter } from 'rxjs';
import { TranslatePipe } from '@ngx-translate/core';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AuthService } from '@features/identity/services/auth.service';
import { CONFIGURATOR_PATH, LEGACY_FILTER_PATH } from '@app/features/configurator/guards/configurator-guards';
import { SAVED_LISTS_PATH } from '@app/features/configurator/models/saved-lists';
import { MEMBERS_SEGMENT, ORGANIZATION_PATH } from '@app/features/organization/models/organization';
import { ACCOUNT_PATH } from '@app/features/identity/models/account';
import { SETTINGS_PATH } from '@app/features/settings/models/user-preferences';

/**
 * Main navigation and account actions of the header (Handoff §3.1; SPEC R-SHL-02, R-SHL-03). `bar`
 * is the row of the full header (from 600 px), `drawer` the content of the compact menu.
 *
 * The three states come from `withIdentity()`: Anonymous shows „Anmelden“; SignedIn shows the
 * organisation display and the menu „Konto“; SignedInNoOrg the menu alone (S5-18). „Projekte“
 * comes with the projects screens of the workspace step (owner decision S5-18 1a). With more than
 * one membership the display is the organisation switch: in the bar a menu button whose name is
 * its visible text, with one `menuitemradio` per organisation (a select would change the context
 * on input, WCAG 3.2.2); in the drawer a list of buttons, the active one `aria-current`. The menu
 * „Konto“ (`shell.userMenu.label`) holds „Konto“, „Organisation“ (S21 of the active organisation,
 * S20 without one; decision 2a), „Einstellungen“ and „Abmelden“; the drawer lists them flat.
 *
 * „Kriterien“ links S10; without an active catalog release its guard opens the legacy filter
 * instead (O-S4-3), so the item counts as current on both.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §3.1, §7
 */
@Component({
  selector: 'pa-main-nav',
  imports: [MatButtonModule, MatIconModule, MatMenuModule, RouterLink, RouterLinkActive, TranslatePipe],
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
    const path = this.currentPath();
    return [CONFIGURATOR_PATH, LEGACY_FILTER_PATH].some((base) => path === base || path.startsWith(`${base}/`));
  });

  /** From the session of `withIdentity()` (S5-08): follows sign-in, sign-out and account deletion. */
  protected readonly signedIn = this.store.signedIn;

  /** `{{displayName}}` of `shell.userMenu.label`: the address, the only name an account has. */
  protected readonly displayName = computed(() => this.store.session()?.email ?? '');

  /** The user's organisations by name; more than one makes the display the switch (R-SHL-03). */
  protected readonly organizations = this.store.organizations;

  /** The organisation of the display and the switch; null in the state SignedInNoOrg (R-SHL-02). */
  protected readonly activeOrganization = this.store.activeOrganization;

  /** „Organisation“: S21 of the active organisation, or S20 without one (owner decision S5-18 2a). */
  protected readonly organizationLink = computed(() => {
    const active = this.activeOrganization();
    return active ? ['/', ORGANIZATION_PATH, active.id, MEMBERS_SEGMENT] : ['/', ORGANIZATION_PATH];
  });

  /** „Kriterienlisten“ (U140, owner decision S5-20b 4a): S14, only with a membership. */
  protected readonly savedListsLink = `/${SAVED_LISTS_PATH}`;

  /** „Einstellungen“ (U138, U140): S60, the last entry before „Abmelden“. */
  protected readonly accountLink = `/${ACCOUNT_PATH}`;
  protected readonly settingsLink = `/${SETTINGS_PATH}`;

  /** S20, S21 and the page of the invitation link count as „Organisation“. */
  protected readonly organizationCurrent = computed(() => {
    this.navigationEnd();
    const path = this.currentPath();
    return path === `/${ORGANIZATION_PATH}` || path.startsWith(`/${ORGANIZATION_PATH}/`);
  });

  /**
   * Makes another organisation the active one (R-SHL-03). On S21 the page follows: it opens S21 of
   * the chosen organisation, which loads its data and is announced as a page change. S14 reads the
   * active organisation and loads its lists (S5-20b); projects and the compare select read it when
   * they are built (workspace and directory steps).
   */
  protected async switchTo(organizationId: string): Promise<void> {
    this.navigated.emit();
    if (organizationId === this.activeOrganization()?.id) {
      return;
    }
    this.store.switchOrganization(organizationId);
    const segments = this.currentPath().split('/');
    if (segments.length === 4 && segments[1] === ORGANIZATION_PATH && segments[3] === MEMBERS_SEGMENT) {
      await this.router.navigate(['/', ORGANIZATION_PATH, organizationId, MEMBERS_SEGMENT]);
    }
  }

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

  /** The path of the primary outlet, without query, fragment and the `login` outlet. */
  private currentPath(): string {
    return this.router.url.split(/[?#(]/)[0];
  }
}
