// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT, Location } from '@angular/common';
import { inject, Injectable } from '@angular/core';
import { NavigationError, RedirectCommand, Router } from '@angular/router';

import { ConnectionStatus, SERVER_ERROR_PATH } from './system-errors';

/**
 * What the browser engines say when the dynamic `import()` of a route chunk fails because the file
 * is gone (a deployment since the tab was opened) or cannot be fetched: Chromium, Firefox, WebKit.
 * Matched on the message, because the engines give no error code; a new engine's text goes here.
 */
const CHUNK_LOAD_MESSAGES: readonly RegExp[] = [
  /Failed to fetch dynamically imported module/i,
  /error loading dynamically imported module/i,
  /Importing a module script failed/i,
];

/** Key in the state of the history entry that names the target the last reload was for (#296). */
export const CHUNK_RELOAD_STATE_KEY = 'paChunkReload';

/** Whether a navigation failed because a lazy route chunk could not be loaded (#296). */
export function isChunkLoadError(error: unknown): boolean {
  return error instanceof TypeError && CHUNK_LOAD_MESSAGES.some((message) => message.test(error.message));
}

/**
 * What a failed chunk leads to (owner decision O-S6-5 (a)):
 * - `stay`: offline — no reload; the offline notice shows and the user tries again (#296 point 4)
 * - `errorPage`: this history entry was already reloaded for the same target — S80 instead of a loop
 * - `reload`: load the target in full once, so the browser fetches the new `index.html` and chunks
 */
export type ChunkFailureAction = 'stay' | 'errorPage' | 'reload';

/** The guard of the reload: one per target and history entry, none while offline. */
export function chunkFailureAction(target: string, reloadedFor: string | null, offline: boolean): ChunkFailureAction {
  if (offline) return 'stay';
  return reloadedFor === target ? 'errorPage' : 'reload';
}

/**
 * The full reload of a navigation target, guarded by the session history: the history entry of
 * the target carries {@link CHUNK_RELOAD_STATE_KEY}, which survives the reload (the browser keeps
 * an entry's state), and nothing goes into browser storage (R-CON-09, R-SET-02). It writes through
 * Angular's `Location`, the history the router uses.
 */
@Injectable({ providedIn: 'root' })
export class DocumentReload {
  private readonly location = inject(Location);
  private readonly window = inject(DOCUMENT).defaultView;

  /** The router URL the current history entry was reloaded for, or null. */
  reloadedFor(): string | null {
    const state: unknown = this.location.getState();
    if (typeof state !== 'object' || state === null) return null;
    const target = (state as Record<string, unknown>)[CHUNK_RELOAD_STATE_KEY];
    return typeof target === 'string' ? target : null;
  }

  /**
   * Marks the history entry of `target` (a router URL) and reloads it. Deferred to the next task:
   * after the error handler returns, the router puts the address and state of the previous page
   * back (`NavigationError` → restore of the history) in the same call stack, and the browser
   * reloads only later — written at once, the mark and the target would be overwritten. An entry
   * that already shows the target keeps its state and gets the mark; any other target gets a new
   * entry, so „Zurück“ leads to the page the user came from.
   */
  reload(target: string): void {
    setTimeout(() => {
      if (this.location.isCurrentPathEqualTo(target)) {
        this.location.replaceState(target, '', { ...(this.location.getState() as object | null), [CHUNK_RELOAD_STATE_KEY]: target });
      } else {
        this.location.go(target, '', { [CHUNK_RELOAD_STATE_KEY]: target });
      }
      this.reloadDocument();
    });
  }

  /** The browser's reload of the current entry; a seam for the tests. */
  protected reloadDocument(): void {
    this.window?.location.reload();
  }
}

/**
 * The router's navigation error handler (`withNavigationErrorHandler`, #296): a lazy route chunk
 * that cannot be loaded — typically in a tab opened before a deployment — reloads the target once;
 * if the chunk fails again after that reload, S80 „Etwas ist schiefgelaufen“ shows in place (URL
 * kept). Offline nothing happens: the offline notice stays and the user tries again. The in-memory
 * draft of S10/S11 is lost on the reload, as on every reload (owner decision O-S6-5). Every other
 * navigation error keeps the router's default (logged, navigation cancelled).
 */
export function handleChunkLoadError(error: NavigationError): RedirectCommand | void {
  if (!isChunkLoadError(error.error)) return;
  const reload = inject(DocumentReload);
  const offline = inject(ConnectionStatus).offline();
  switch (chunkFailureAction(error.url, reload.reloadedFor(), offline)) {
    case 'reload':
      reload.reload(error.url);
      return;
    case 'errorPage': {
      const router = inject(Router);
      return new RedirectCommand(router.createUrlTree(['/', SERVER_ERROR_PATH]), { skipLocationChange: true });
    }
    default:
      return;
  }
}
