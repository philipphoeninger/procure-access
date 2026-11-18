// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Routes } from '@angular/router';
import { Home } from '@pages/home/home';
import { authGuard } from '@core/guards/auth.guard';
import { CONFIRM_EMAIL_PATH, LOGIN_PATH, NEW_PASSWORD_PATH, REGISTER_PATH, RESET_PASSWORD_PATH } from '@features/identity/models/login-navigation';
import { ERROR_PAGE_KEY, ErrorPage } from './pages/error/error-page';
import { PAGE_TITLE_KEY } from './core/a11y/page-title.strategy';
import { SERVER_ERROR_PATH } from './core/errors/system-errors';
import { completeSelectionGuard, configuratorReleaseGuard } from './features/configurator/guards/configurator-guards';
import { INVITATION_SEGMENT, MEMBERS_SEGMENT, ORGANIZATION_PATH } from './features/organization/models/organization';
import { SAVED_LISTS_PATH } from './features/configurator/models/saved-lists';
import { SHARED_PATH } from './features/configurator/models/share-links';
import { ACCOUNT_PATH } from './features/identity/models/account';
import { SETTINGS_PATH } from './features/settings/models/user-preferences';
import { PROJECTS_PATH } from './features/workspace/models/projects';

// `data[PAGE_TITLE_KEY]` names the page title (R-SHL-04, UX copy §5.16). Legacy screens without a
// matching key keep the app name as title until their screen is rebuilt (S2-16).
// Feature pages load with their route (`loadComponent`), so the first download holds only the shell,
// the start page and the error pages; the initial budget in angular.json counts that (#42). The
// legal pages load with their route too since their Markdown texts are bundled with them (S6-28).
const legalPage = () => import('./pages/legal/legal-page').then((m) => m.LegalPage);

export const routes: Routes = [
  // S04 start page (R-SHL-05); `/home` is the legacy address
  {
    path: '',
    pathMatch: 'full',
    component: Home,
    data: { [PAGE_TITLE_KEY]: 'pageTitle.home' },
  },
  {
    path: 'home',
    pathMatch: 'full',
    redirectTo: '',
  },
  // S01 „Anmelden“ (R-AUTH-03) in the `login` outlet inside the shell, over the page that opened it;
  // `auth` is the legacy path, redirected until S5-10 removes its last callers
  {
    path: LOGIN_PATH,
    loadComponent: () => import('@features/identity/pages/login/login-page').then((m) => m.LoginPage),
    outlet: 'login',
    data: { [PAGE_TITLE_KEY]: 'pageTitle.login' },
  },
  {
    path: 'auth',
    redirectTo: LOGIN_PATH,
    outlet: 'login',
  },
  // S02 „Konto erstellen“ (R-AUTH-01) in place of the page; `register` is the legacy path
  {
    path: REGISTER_PATH,
    loadComponent: () => import('@features/identity/pages/register/register-page').then((m) => m.RegisterPage),
    data: { [PAGE_TITLE_KEY]: 'pageTitle.register' },
  },
  {
    path: 'register',
    pathMatch: 'full',
    redirectTo: REGISTER_PATH,
  },
  // The confirmation page of the e-mail link (R-AUTH-02): its guard takes the token out of the URL,
  // its resolver redeems it and names the page title of the result (lazy child routes, see there)
  {
    path: CONFIRM_EMAIL_PATH,
    loadChildren: () => import('@features/identity/pages/confirm-email/confirm-email.routing').then((m) => m.CONFIRM_EMAIL_ROUTES),
  },
  // S03 „Passwort zurücksetzen“ (R-AUTH-04) in place of the page: the request step, and the
  // new-password step of the e-mail link, whose guard takes the token out of the URL (lazy child
  // routes like the confirmation page); `reset-password` is the legacy path of older e-mails
  {
    path: RESET_PASSWORD_PATH,
    pathMatch: 'full',
    loadComponent: () => import('@features/identity/pages/reset-password/reset-request-page').then((m) => m.ResetRequestPage),
    data: { [PAGE_TITLE_KEY]: 'pageTitle.reset' },
  },
  {
    path: NEW_PASSWORD_PATH,
    loadChildren: () => import('@features/identity/pages/new-password/new-password.routing').then((m) => m.NEW_PASSWORD_ROUTES),
  },
  {
    path: 'reset-password',
    pathMatch: 'full',
    redirectTo: RESET_PASSWORD_PATH,
  },
  // S61 account (R-AUTH-09, R-AUTH-10): the export download and the account deletion; signed in only
  {
    path: ACCOUNT_PATH,
    loadChildren: () => import('./features/identity/pages/account/account.routing').then((m) => m.ACCOUNT_ROUTES),
  },
  // S10 configurator (R-CON-01): start page and header link it; without an active catalog release
  // the guard opens the legacy /filters instead (O-S4-3). The page fills the step number of its title.
  {
    path: 'kriterien',
    loadComponent: () => import('./features/configurator/pages/criteria-select/criteria-select').then((m) => m.CriteriaSelect),
    canActivate: [configuratorReleaseGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.step' },
  },
  // S11 (R-CON-10): placeholder until S4-14 – S4-18; only with a complete selection from S10
  {
    path: 'kriterien/ergebnis',
    loadComponent: () => import('./features/configurator/pages/criteria-list/criteria-list').then((m) => m.CriteriaListPage),
    canActivate: [completeSelectionGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.list' },
  },
  // S14 „Gespeicherte Kriterienlisten“ (R-SAV-02, S5-20b): the lists of the organisation the
  // header shows; signed in only
  {
    path: SAVED_LISTS_PATH,
    pathMatch: 'full',
    loadComponent: () => import('./features/configurator/pages/saved-lists/saved-lists-page').then((m) => m.SavedListsPage),
    canActivate: [authGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.saved' },
  },
  // A saved list, S11 read only until S5-21 (owner decision S5-20 5a): its resolver loads the list
  // and names the page title (lazy child routes like S21)
  {
    path: `${SAVED_LISTS_PATH}/:id`,
    loadChildren: () => import('./features/configurator/pages/saved-list/saved-list.routing').then((m) => m.SAVED_LIST_ROUTES),
  },
  // S16 „Geteilte Kriterienliste“ (R-SAV-05, S5-24): the list behind a share link, without an
  // account (TEN-09); its resolver loads it and names the page title
  {
    path: `${SHARED_PATH}/:token`,
    loadChildren: () => import('./features/configurator/pages/shared-list/shared-list.routing').then((m) => m.SHARED_LIST_ROUTES),
  },
  // S30 „Beschaffungsprojekte“ (R-PRJ-01, S6-13): the projects of the organisation the header
  // shows; signed in only; „Projekte“ in the header leads here (R-SHL-02)
  {
    path: PROJECTS_PATH,
    pathMatch: 'full',
    loadComponent: () => import('./features/workspace/pages/projects/projects-page').then((m) => m.ProjectsPage),
    canActivate: [authGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.projects' },
  },
  // S32 of a project, the placeholder of S6-13 until S6-14: its resolver loads the project and
  // names the page title (lazy child routes like a saved list)
  {
    path: `${PROJECTS_PATH}/:id`,
    loadChildren: () => import('./features/workspace/pages/project-detail/project-detail.routing').then((m) => m.PROJECT_ROUTES),
  },
  {
    path: SETTINGS_PATH,
    loadChildren: () => import('./features/settings/pages/settings/settings.routing').then((m) => m.SETTINGS_ROUTES),
  },
  {
    path: 'filters',
    loadComponent: () => import('@features/products/pages/products-container/products-container').then((m) => m.ProductsContainer)
  },
  {
    path: 'favorites',
    loadComponent: () => import('@app/features/favorites/list/favorites').then((m) => m.Favorites),
    canActivate: [authGuard],
  },
  {
    path: 'products',
    loadComponent: () => import('@app/features/products/list/products-list').then((m) => m.ProductsList)
  },
  { 
    path: 'product/:id', 
    loadComponent: () => import('@app/features/products/details/product-details').then((m) => m.ProductDetails)
   },
  // S20 „Organisation anlegen oder beitreten“ and S21 „Mitglieder“ (R-ORG-01, R-ORG-03, R-ORG-04;
  // signed in only; no menu entry until S5-18, owner decision S5-16 6a). S21 loads its members
  // through a resolver, so its title names the organisation at the route change.
  {
    path: ORGANIZATION_PATH,
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/organization/pages/organization-start/organization-start-page').then((m) => m.OrganizationStartPage),
    canActivate: [authGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.org.start' },
  },
  // The page of an invitation link (R-ORG-02, S5-17b): no sign-in needed; its guard takes the
  // token out of the URL and its resolvers read the invitation (lazy child routes like the
  // confirmation page)
  {
    path: `${ORGANIZATION_PATH}/${INVITATION_SEGMENT}`,
    loadChildren: () => import('./features/organization/pages/invitation/invitation.routing').then((m) => m.INVITATION_ROUTES),
  },
  {
    path: `${ORGANIZATION_PATH}/:id/${MEMBERS_SEGMENT}`,
    loadChildren: () => import('./features/organization/pages/members/members.routing').then((m) => m.MEMBERS_ROUTES),
  },
  // S80 error pages (R-SHL-08); the page title is the H1
  {
    path: 'unauthorized',
    component: ErrorPage,
    data: { [ERROR_PAGE_KEY]: 'forbidden', [PAGE_TITLE_KEY]: 'pageTitle.forbidden' },
  },
  {
    path: SERVER_ERROR_PATH,
    component: ErrorPage,
    data: { [ERROR_PAGE_KEY]: 'server', [PAGE_TITLE_KEY]: 'pageTitle.server' },
  },
  // S71 legal pages (R-SHL-07, S6-28): Markdown from content/legal/*.de.md, bundled into their own
  // chunk; reachable without an account (R-SHL-06)
  { path: 'impressum', loadComponent: legalPage, data: { legalPage: 'imprint', [PAGE_TITLE_KEY]: 'pageTitle.legal.imprint' } },
  { path: 'datenschutz', loadComponent: legalPage, data: { legalPage: 'privacy', [PAGE_TITLE_KEY]: 'pageTitle.legal.privacy' } },
  { path: 'barrierefreiheit', loadComponent: legalPage, data: { legalPage: 'accessibility', [PAGE_TITLE_KEY]: 'pageTitle.legal.accessibility' } },
  // Every other address: „Seite nicht gefunden“ in place, the URL stays (must stay last)
  { path: '**', component: ErrorPage, data: { [ERROR_PAGE_KEY]: 'notFound', [PAGE_TITLE_KEY]: 'pageTitle.notFound' } },
];
