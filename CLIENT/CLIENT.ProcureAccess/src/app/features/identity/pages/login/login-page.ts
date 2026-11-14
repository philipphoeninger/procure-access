// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { Notice } from '@app/shared/components/notice/notice';
import { LoginForm } from '../../components/login-form/login-form';
import { LoginNavigationState } from '../../models/login-navigation';

/**
 * S01 „Anmelden“ in the `login` outlet (SPEC R-AUTH-03; UX copy §3, §5.2; Handoff §5.3: no Figma
 * frame, form card as in S51). The outlet sits in `main` of the shell and hides the page it opened
 * over, which stays alive with its input (ARCHITECTURE §8.3, owner decision S5-09 2 (a)).
 *
 * H1, `auth.login.reasonSave` when S11 „Liste speichern“ opened it (router state, never the URL),
 * the sign-in form, and `auth.login.noAccount` to the registration. After the sign-in the outlet
 * closes, the page underneath gets the focus on its H1 (route change, A11Y-03), and with a list
 * that is not saved `auth.login.draftKept` is announced. „Fehler:“ prefixes the page title while
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

  /** S11 „Liste speichern“ opened S01 (`auth.login.reasonSave`); read once, while the navigation runs. */
  protected readonly reasonSave =
    (this.router.currentNavigation()?.extras.state as LoginNavigationState | undefined)?.loginReason === 'save';

  protected showTitleError(error: boolean): void {
    this.pageTitle.setPageTitleState({ error });
  }

  /** Closes the outlet: the page underneath shows again with everything the user entered. */
  protected async signedIn(): Promise<void> {
    // The list of S11 is not saved before step 3 saves lists, so any loaded list counts (E7).
    const draftKept = this.reasonSave && this.store.configurator.list.answer() !== null;
    await this.router.navigate([{ outlets: { login: null } }]);
    if (draftKept) {
      void this.announcer.announce('auth.login.draftKept');
    }
  }
}
