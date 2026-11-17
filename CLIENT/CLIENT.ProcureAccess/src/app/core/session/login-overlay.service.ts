// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable, Injector } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';

/**
 * Opens the login overlay after a 401 (ARCHITECTURE §8.3, C60; SPEC R-AUTH-07; SEC-03): the sign-in
 * form in a dialog, without navigation, so the page, its input and the configurator draft stay.
 * Several requests failing at once share one dialog and one answer. The component loads with its
 * first use, so the first download stays without the form (#42).
 */
@Injectable({ providedIn: 'root' })
export class LoginOverlayService {
  private readonly dialog = inject(MatDialog);
  private readonly injector = inject(Injector);
  private pending: Promise<boolean> | null = null;

  /**
   * Resolves true once the user of the ended session signed in again, false when the overlay was
   * closed instead or another account signed in (owner decision S5-30 D3 (b)). The session warning
   * passes the previous user, whose session its probe already cleared; after a 401 the overlay
   * reads it from the store. A request while the overlay is open shares it.
   */
  requestSignIn(previousUserId?: string | null): Promise<boolean> {
    this.pending ??= this.open(previousUserId).finally(() => {
      this.pending = null;
    });
    return this.pending;
  }

  private async open(previousUserId: string | null | undefined): Promise<boolean> {
    const { LoginOverlay } = await import('@features/identity/dialogs/login-overlay/login-overlay');
    const data = previousUserId === undefined ? {} : { previousUserId };
    const ref = await LoginOverlay.open(this.dialog, this.injector, data);
    const result = await firstValueFrom(ref.afterClosed());
    return result === true;
  }
}
