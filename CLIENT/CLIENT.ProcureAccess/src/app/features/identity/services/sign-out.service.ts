// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';

/**
 * The end of a sign-out as the user sees it (R-AUTH-08): every in-memory state of the user goes and
 * the start page shows with the focus on its H1 — also when it was already open, where the router
 * sees no page change. „Abmelden“ in the menu „Konto“, „Jetzt abmelden“ in the session warning
 * (R-AUTH-06) and another account signed in through the login overlay (owner decision S5-30 D3 (b))
 * end there.
 */
@Injectable({ providedIn: 'root' })
export class SignOutService {
  private readonly store = inject(ProcureAccessStore);
  private readonly router = inject(Router);
  private readonly pageTitle = inject(PageTitleStrategy);

  /** Ends the session and shows the start page; the session is forgotten also when the request fails. */
  async signOut(): Promise<void> {
    // The error interceptor shows a failed request.
    await this.store.endSession().catch(() => undefined);
    await this.showStartPage();
  }

  /** Clears the previous user's data, keeps the new session and shows the start page (D3 (b)). */
  async anotherUserSignedIn(): Promise<void> {
    this.store.clearUserData();
    await this.showStartPage();
  }

  private async showStartPage(): Promise<void> {
    await this.router.navigateByUrl('/');
    this.pageTitle.focusPageHeading();
  }
}
