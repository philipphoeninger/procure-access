// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable } from '@angular/core';
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
  private pending: Promise<boolean> | null = null;

  /** Resolves true once the user signed in, false when the overlay was closed instead. */
  requestSignIn(): Promise<boolean> {
    this.pending ??= this.open().finally(() => {
      this.pending = null;
    });
    return this.pending;
  }

  private async open(): Promise<boolean> {
    const { LoginOverlay } = await import('@features/identity/dialogs/login-overlay/login-overlay');
    const result = await firstValueFrom(LoginOverlay.open(this.dialog).afterClosed());
    return result === true;
  }
}
