// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DestroyRef, inject, Injectable, signal } from '@angular/core';
import { DOCUMENT } from '@angular/common';

/** Route of the S80 page „Etwas ist schiefgelaufen“; the error interceptor shows it in place. */
export const SERVER_ERROR_PATH = 'error';

/**
 * Whether the server is out of reach (Handoff §6.5, `errors.offline`). Offline when a request
 * gets no answer at all (status 0) or the browser goes offline; online again with the next
 * answer from the API (not from a static file, #171) or when the browser is back online.
 *
 * @spec docs/SPEC_v2.md R-SHL-08; docs/design_handoff_procureaccess_v1.md §6.5
 */
@Injectable({ providedIn: 'root' })
export class ConnectionStatus {
  private readonly window = inject(DOCUMENT).defaultView;
  private readonly offlineState = signal(this.window?.navigator.onLine === false);

  readonly offline = this.offlineState.asReadonly();

  constructor() {
    const toOffline = () => this.offlineState.set(true);
    const toOnline = () => this.offlineState.set(false);
    this.window?.addEventListener('offline', toOffline);
    this.window?.addEventListener('online', toOnline);
    inject(DestroyRef).onDestroy(() => {
      this.window?.removeEventListener('offline', toOffline);
      this.window?.removeEventListener('online', toOnline);
    });
  }

  /** A request got no answer. */
  noAnswer(): void {
    this.offlineState.set(true);
  }

  /** The API answered (with any status). */
  answered(): void {
    this.offlineState.set(false);
  }
}
