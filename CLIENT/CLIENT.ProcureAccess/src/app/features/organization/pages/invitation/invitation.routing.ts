// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { CanActivateFn, RedirectCommand, ResolveFn, Router, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { problemTypeName } from '@app/core/errors/problem-details';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { INVITATION_SEGMENT, MEMBERS_SEGMENT, ORGANIZATION_PATH } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { InvitationPage } from './invitation-page';
import { INVITATION_VIEW_KEY, InvitationLoad, InvitationPageState, InvitationView } from './invitation-page-state';

/**
 * Takes the token out of the address before anything is sent (as `confirmEmailTokenGuard`): it
 * keeps the token in memory and opens the same page without the query, replacing the history
 * entry, so the token is neither in the browser history nor in a `Referer`.
 */
export const invitationTokenGuard: CanActivateFn = (route) => {
  const token = route.queryParamMap.get('token');
  if (token === null) {
    return true;
  }
  inject(InvitationPageState).token = token.length > 0 ? token : null;
  const router = inject(Router);
  // `reload`: a second link opened on the page redirects to the address it already shows
  return new RedirectCommand(router.createUrlTree([`/${ORGANIZATION_PATH}/${INVITATION_SEGMENT}`]), {
    replaceUrl: true,
    onSameUrlNavigation: 'reload',
  });
};

/**
 * Reads what the link invites to (SPEC §8.9 `POST /invitations/view`), once per navigation. It
 * waits for the session probe, which issues the antiforgery token the request needs and tells
 * whether someone is signed in. 400 `validation` → „nicht mehr gültig“ and the token is forgotten;
 * any other failure → the S80 server text, because the link may still be valid. The loading bar
 * shows meanwhile.
 */
function loadInvitation(): Promise<InvitationLoad> {
  const state = inject(InvitationPageState);
  const api = inject(OrganizationApiService);
  const store = inject(ProcureAccessStore);
  const navigationId = inject(Router).currentNavigation()?.id ?? -1;
  return state.loadFor(navigationId, async () => {
    const token = state.token;
    if (token === null) {
      return { kind: 'invalid' };
    }
    store.incrementLoadingCount();
    try {
      await store.sessionReady();
      const details = await api.viewInvitation(token);
      if (details.alreadyMember && details.organizationId !== null) {
        state.token = null;
        return { kind: 'member', organizationId: details.organizationId };
      }
      return { kind: 'open', token, details };
    } catch (error: unknown) {
      if (problemTypeName(error) === 'validation') {
        state.token = null;
        return { kind: 'invalid' };
      }
      return { kind: 'server' };
    } finally {
      store.decrementLoadingCount();
    }
  });
}

/** The view for the page; a member who opens the link again goes to the members page S21 (D7 (a)). */
export const invitationViewResolver: ResolveFn<InvitationView> = async () => {
  const router = inject(Router);
  const load = await loadInvitation();
  if (load.kind === 'member') {
    return new RedirectCommand(router.createUrlTree(['/', ORGANIZATION_PATH, load.organizationId, MEMBERS_SEGMENT]), { replaceUrl: true });
  }
  return load;
};

/** The page title of the view (UX copy §5.16, U133); the route change focuses the H1 and announces it (A11Y-03). */
export const invitationTitleResolver: ResolveFn<string> = async () => {
  switch ((await loadInvitation()).kind) {
    case 'open':
      return 'pageTitle.org.invite';
    case 'invalid':
      return 'pageTitle.org.invite.invalid';
    case 'member':
      return 'pageTitle.org.members';
    default:
      return 'pageTitle.server';
  }
};

/**
 * The child routes of `/organisation/einladung`, loaded with it (`loadChildren` in app.routes.ts):
 * guard and resolvers need the store and the API, which app.routes.ts must not import. No sign-in
 * needed (owner decision S5-17 D5 (a)). Guard and resolvers run on every navigation to the page: a
 * second link brings its token, and closing S01 after a sign-in reads the invitation again for the
 * signed-in address.
 */
export const INVITATION_ROUTES: Routes = [
  {
    path: '',
    component: InvitationPage,
    canActivate: [invitationTokenGuard],
    resolve: { [PAGE_TITLE_KEY]: invitationTitleResolver, [INVITATION_VIEW_KEY]: invitationViewResolver },
    runGuardsAndResolvers: 'always',
  },
];
