// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { LoginForm } from '../../components/login-form/login-form';
import { LoginNavigationState, REGISTER_PATH } from '../../models/login-navigation';
import { safeReturnUrl } from '../../models/return-url';

/**
 * S01 „Anmelden“ in the `login` outlet (SPEC R-AUTH-03; UX copy §3, §5.2; Handoff §5.3: no Figma
 * frame, form card as in S51). The outlet sits in `main` of the shell and hides the page it opened
 * over, which stays alive with its input (ARCHITECTURE §8.3, owner decision S5-09 2 (a)).
 *
 * H1, `auth.login.reasonSave` when S11 „Liste speichern“ opened it (router state, never the URL),
 * the sign-in form, and `auth.login.noAccount` to S02, which opens in place of the page and closes
 * the outlet (S5-12b). After the sign-in the outlet
 * closes, the page underneath gets the focus on its H1 (route change, A11Y-03), and with a list
 * that is not saved `auth.login.draftKept` is announced. When `authGuard` opened S01 for a guarded
 * page, the router state carries that page as `returnUrl` and S01 goes there instead, if it is a
 * path of this app (`safeReturnUrl`, S6-03). „Fehler:“ prefixes the page title while
 * the error summary shows.
 *
 * @spec docs/SPEC_v2.md R-AUTH-03; docs/design_handoff_procureaccess_v1.md §5.3, §6.4
 * @invariants A11Y-03
 */
@Component({
  selector: 'pa-login-page',
  imports: [LoginForm, Notice, RouterLink, TranslatePipe],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginPage {
  private readonly router = inject(Router);
  private readonly store = inject(ProcureAccessStore);
  private readonly announcer = inject(A11yAnnouncerService);
  private readonly pageTitle = inject(PageTitleStrategy);

  protected readonly registerPath = REGISTER_PATH;

  /**
   * Router state of the navigation that opened S01, read once. The outlet creates S01 while that
   * navigation runs, or right after it when the shell's outlet did not exist yet (a deep link that
   * `authGuard` redirects during the first navigation), then it is the last successful one.
   */
  private readonly navigationState = (this.router.currentNavigation() ?? this.router.lastSuccessfulNavigation())?.extras
    .state as LoginNavigationState | undefined;

  /** S11 „Liste speichern“ opened S01 (`auth.login.reasonSave`). */
  protected readonly reasonSave = this.navigationState?.loginReason === 'save';

  /** The guarded page `authGuard` sent the user away from, if it is a path of this app (S6-03). */
  private readonly returnUrl = safeReturnUrl(this.navigationState?.returnUrl, inject(DOCUMENT).location.origin);

  protected showTitleError(error: boolean): void {
    this.pageTitle.setPageTitleState({ error });
  }

  /**
   * Opens the guarded page the user came for, or closes the outlet: the page underneath shows again
   * with everything the user entered.
   */
  protected async signedIn(): Promise<void> {
    if (this.returnUrl !== null) {
      // The URL has no `login` outlet, so this navigation closes S01 too.
      await this.router.navigateByUrl(this.returnUrl);
      return;
    }
    // The draft of S11 lives only in memory, so any loaded list is unsaved (E7). S11 also shows the
    // text, until the list is saved or S11 is left (owner decision S5-20b 1a).
    const draftKept = this.reasonSave && this.store.configurator.list.answer() !== null;
    if (draftKept) {
      this.store.keepDraftAfterSignIn();
    }
    await this.router.navigate([{ outlets: { login: null } }]);
    if (draftKept) {
      void this.announcer.announce('auth.login.draftKept');
    }
  }
}
