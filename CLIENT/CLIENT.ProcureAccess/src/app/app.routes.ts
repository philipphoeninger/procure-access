// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Routes } from '@angular/router';
import { Home } from '@pages/home/home';
import { AuthGuard } from '@core/guards/auth.guard';
import { PermissionGuard } from './features/identity/guards/permission.guard';
import { ERROR_PAGE_KEY, ErrorPage } from './pages/error/error-page';
import { LegalPage } from './pages/legal/legal-page';
import { PAGE_TITLE_KEY } from './core/a11y/page-title.strategy';
import { SERVER_ERROR_PATH } from './core/errors/system-errors';
import { completeSelectionGuard, configuratorReleaseGuard } from './features/configurator/guards/configurator-guards';

// `data[PAGE_TITLE_KEY]` names the page title (R-SHL-04, UX copy §5.16). Legacy screens without a
// matching key keep the app name as title until their screen is rebuilt (S2-16).
// Feature pages load with their route (`loadComponent`), so the first download holds only the shell,
// the start page and the error and legal pages; the initial budget in angular.json counts that (#42).
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
  {
    path: 'auth',
    loadComponent: () => import('@features/identity/login/login').then((m) => m.Login),
    outlet: 'login',
  },
  {
    path: 'register',
    loadComponent: () => import('@features/identity/register/register').then((m) => m.Register),
    outlet: 'login',
  },
  {
    path: 'proposals',
    loadComponent: () => import('./features/proposal/list/proposal-list').then((m) => m.ProposalList),
    canActivate: [PermissionGuard],
    data: { permission: 'objects:approve' }
  },
  {
    path: 'profile',
    loadComponent: () => import('@features/identity/profile/profile').then((m) => m.Profile),
    canActivate: [AuthGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.account' },
  },
  {
    path: 'settings',
    loadComponent: () => import('@features/settings/settings').then((m) => m.Settings),
    data: { [PAGE_TITLE_KEY]: 'pageTitle.settings' },
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
  {
    path: 'filters',
    loadComponent: () => import('@features/products/pages/products-container/products-container').then((m) => m.ProductsContainer)
  },
  {
    path: 'favorites',
    loadComponent: () => import('@app/features/favorites/list/favorites').then((m) => m.Favorites),
    canActivate: [AuthGuard],
  },
  {
    path: 'products',
    loadComponent: () => import('@app/features/products/list/products-list').then((m) => m.ProductsList)
  },
  { 
    path: 'product/:id', 
    loadComponent: () => import('@app/features/products/details/product-details').then((m) => m.ProductDetails)
   },
   {
    path: 'product-proposal/:id',
    loadComponent: () => import('@app/features/products/proposal/product-proposal').then((m) => m.ProductProposal),
    canActivate: [AuthGuard]
   },
   {
    path: 'criterion-proposal/:id',
    loadComponent: () => import('@app/features/criteria/proposal/criterion-proposal').then((m) => m.CriterionProposal),
    canActivate: [AuthGuard]
   },
  { 
    path: 'reset-password', 
    loadComponent: () => import('./features/identity/reset-password/reset-password').then((m) => m.ResetPassword)
  },
  { 
    path: 'confirm-email', 
    loadComponent: () => import('./features/identity/pages/email-confirmed/email-confirmation').then((m) => m.EmailConfirmation)
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
  // S71 legal pages (R-SHL-07): placeholders until GP.3; reachable without an account (R-SHL-06)
  { path: 'impressum', component: LegalPage, data: { legalPage: 'imprint', [PAGE_TITLE_KEY]: 'pageTitle.legal.imprint' } },
  { path: 'datenschutz', component: LegalPage, data: { legalPage: 'privacy', [PAGE_TITLE_KEY]: 'pageTitle.legal.privacy' } },
  { path: 'barrierefreiheit', component: LegalPage, data: { legalPage: 'accessibility', [PAGE_TITLE_KEY]: 'pageTitle.legal.accessibility' } },
  // Every other address: „Seite nicht gefunden“ in place, the URL stays (must stay last)
  { path: '**', component: ErrorPage, data: { [ERROR_PAGE_KEY]: 'notFound', [PAGE_TITLE_KEY]: 'pageTitle.notFound' } },
];
