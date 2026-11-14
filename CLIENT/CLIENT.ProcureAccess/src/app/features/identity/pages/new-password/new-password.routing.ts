// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable } from '@angular/core';
import { CanActivateFn, RedirectCommand, ResolveFn, Router, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { NEW_PASSWORD_PATH } from '../../models/login-navigation';
import { NewPasswordPage } from './new-password-page';

/** What the new-password step shows: the form with the link's token, the result, or „Link nicht mehr gültig“. */
export type NewPasswordView = { kind: 'form'; token: string } | { kind: 'done' } | { kind: 'invalid' };

/** Route data key of the {@link NewPasswordView}. */
export const NEW_PASSWORD_VIEW_KEY = 'newPasswordView';

/**
 * The state of the new-password step between its guard, its resolvers and the page: the token of
 * the reset link and what the page shows. Memory only — never browser storage, never router state
 * (which the browser keeps in its session history) — so a reload of the page shows „Link nicht
 * mehr gültig“; the link in the e-mail still works then, as on the confirmation page.
 */
@Injectable({ providedIn: 'root' })
export class NewPasswordState {
  view: NewPasswordView = { kind: 'invalid' };
}

/**
 * Takes the token out of the address before anything is sent (as `confirmEmailTokenGuard`): it
 * keeps the token in memory and opens the same page without the query, replacing the history
 * entry, so the token is neither in the browser history nor in a `Referer`.
 */
export const newPasswordTokenGuard: CanActivateFn = (route) => {
  const token = route.queryParamMap.get('token');
  if (token === null) {
    return true;
  }
  inject(NewPasswordState).view = token.length > 0 ? { kind: 'form', token } : { kind: 'invalid' };
  const router = inject(Router);
  // `reload`: a second link opened on the page redirects to the address it already shows
  return new RedirectCommand(router.createUrlTree([`/${NEW_PASSWORD_PATH}`]), { replaceUrl: true, onSameUrlNavigation: 'reload' });
};

/** The view for the page. */
export const newPasswordViewResolver: ResolveFn<NewPasswordView> = () => inject(NewPasswordState).view;

/** The page title of the view: „Link nicht mehr gültig“ or „Passwort zurücksetzen“ (UX copy §5.16, U120, U123). */
export const newPasswordTitleResolver: ResolveFn<string> = () =>
  inject(NewPasswordState).view.kind === 'invalid' ? 'pageTitle.reset.invalid' : 'pageTitle.reset';

/**
 * The child routes of `/passwort/neu`, loaded with it (`loadChildren` in app.routes.ts), like the
 * confirmation page. Guard and resolvers run on every navigation to the page, so a second link
 * opened here brings its token.
 */
export const NEW_PASSWORD_ROUTES: Routes = [
  {
    path: '',
    component: NewPasswordPage,
    canActivate: [newPasswordTokenGuard],
    resolve: { [PAGE_TITLE_KEY]: newPasswordTitleResolver, [NEW_PASSWORD_VIEW_KEY]: newPasswordViewResolver },
    runGuardsAndResolvers: 'always',
  },
];
