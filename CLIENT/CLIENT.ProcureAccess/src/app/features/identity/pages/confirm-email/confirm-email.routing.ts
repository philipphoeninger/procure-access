// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable } from '@angular/core';
import { CanActivateFn, RedirectCommand, ResolveFn, Router, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { CONFIRM_EMAIL_PATH } from '../../models/login-navigation';
import { AuthService } from '../../services/auth.service';
import { ConfirmEmailPage } from './confirm-email-page';

/** What the confirmation page shows; each is the page title key of its result. */
export type ConfirmEmailResult = 'pageTitle.confirm.done' | 'pageTitle.confirm.invalid' | 'pageTitle.server';

/**
 * The token of the confirmation link between the guard that takes it out of the URL and the
 * resolver that redeems it, and the last result. Memory only: never browser storage, never router
 * state (which the browser keeps in its session history).
 */
@Injectable({ providedIn: 'root' })
export class PendingConfirmationToken {
  private token: string | null = null;
  /** The result of the last redeemed token, kept while the app runs (S01 opening over the page). */
  lastResult: ConfirmEmailResult | null = null;

  set(token: string): void {
    this.token = token;
  }

  /** The token, once: a second call returns null. */
  take(): string | null {
    const token = this.token;
    this.token = null;
    return token;
  }
}

/**
 * Takes the token out of the address before anything is sent (S5-12a review R4): it keeps the
 * token in memory and opens the same page without the query, replacing the history entry, so the
 * token is neither in the browser history nor in a `Referer`.
 */
export const confirmEmailTokenGuard: CanActivateFn = (route) => {
  const token = route.queryParamMap.get('token');
  if (token === null) {
    return true;
  }
  inject(PendingConfirmationToken).set(token);
  const router = inject(Router);
  // `reload`: a second link opened on the page redirects to the address it already shows
  return new RedirectCommand(router.createUrlTree([`/${CONFIRM_EMAIL_PATH}`]), { replaceUrl: true, onSameUrlNavigation: 'reload' });
};

/**
 * Redeems the token (SPEC §8.8 `POST /auth/confirm-email`) before the page shows, so its title,
 * the focus on its H1 and the announcement follow the result like any route change (A11Y-03).
 * It waits for the session probe, which issues the antiforgery token the request needs. 204 →
 * „bestätigt“ (also for a link used before for a confirmed address, decision S5-12 4 (b)); 400
 * `validation` or no token (a reload after the guard took it) → „nicht mehr gültig“; any other
 * failure → the S80 server text, because the link may still be valid. The loading bar shows
 * meanwhile. It runs on every navigation to the page (a second link), so without a new token it
 * keeps the last result, e.g. when S01 opens over the page.
 */
export const confirmEmailResultResolver: ResolveFn<ConfirmEmailResult> = async () => {
  const pending = inject(PendingConfirmationToken);
  const token = pending.take();
  if (token === null) {
    return pending.lastResult ?? 'pageTitle.confirm.invalid';
  }
  const store = inject(ProcureAccessStore);
  const authService = inject(AuthService);
  store.incrementLoadingCount();
  let result: ConfirmEmailResult;
  try {
    await store.sessionReady();
    await authService.confirmEmail(token);
    result = 'pageTitle.confirm.done';
  } catch (error: unknown) {
    result = problemTypeName(error) === 'validation' ? 'pageTitle.confirm.invalid' : 'pageTitle.server';
  } finally {
    store.decrementLoadingCount();
  }
  pending.lastResult = result;
  return result;
};

/**
 * The child routes of `/registrieren/bestaetigen`, loaded with it (`loadChildren` in app.routes.ts):
 * the guard and the resolver need the store and the API, which app.routes.ts must not import
 * (they import app.config.ts, which imports the routes).
 */
export const CONFIRM_EMAIL_ROUTES: Routes = [
  {
    path: '',
    component: ConfirmEmailPage,
    canActivate: [confirmEmailTokenGuard],
    resolve: { [PAGE_TITLE_KEY]: confirmEmailResultResolver },
    // A second link opened on this page: guard and resolver run again, also on the reload to the
    // same address that the guard's redirect is
    runGuardsAndResolvers: 'always',
  },
];
