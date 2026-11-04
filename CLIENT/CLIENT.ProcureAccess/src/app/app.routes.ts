// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Routes } from '@angular/router';
import { Login } from '@features/identity/login/login';
import { Register } from '@features/identity/register/register';
import { Home } from '@pages/home/home';
import { AuthGuard } from '@core/guards/auth.guard';
import { Profile } from '@features/identity/profile/profile';
import { Settings } from '@features/settings/settings';
import { ProductsContainer } from '@features/products/pages/products-container/products-container';
import { Favorites } from '@app/features/favorites/list/favorites';
import { ProductsList } from '@app/features/products/list/products-list';
import { ProductDetails } from '@app/features/products/details/product-details';
import { ProductProposal } from '@app/features/products/proposal/product-proposal';
import { CriterionProposal } from '@app/features/criteria/proposal/criterion-proposal';
import { ResetPassword } from './features/identity/reset-password/reset-password';
import { EmailConfirmation } from './features/identity/pages/email-confirmed/email-confirmation';
import { PermissionGuard } from './features/identity/guards/permission.guard';
import { ProposalList } from './features/proposal/list/proposal-list';
import { ERROR_PAGE_KEY, ErrorPage } from './pages/error/error-page';
import { LegalPage } from './pages/legal/legal-page';
import { PAGE_TITLE_KEY } from './core/a11y/page-title.strategy';
import { SERVER_ERROR_PATH } from './core/errors/system-errors';

// `data[PAGE_TITLE_KEY]` names the page title (R-SHL-04, UX copy §5.16). Legacy screens without a
// matching key keep the app name as title until their screen is rebuilt (S2-16).
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
    component: Login,
    outlet: 'login',
  },
  {
    path: 'register',
    component: Register,
    outlet: 'login',
  },
  {
    path: 'proposals',
    component: ProposalList,
    canActivate: [PermissionGuard],
    data: { permission: 'objects:approve' }
  },
  {
    path: 'profile',
    component: Profile,
    canActivate: [AuthGuard],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.account' },
  },
  {
    path: 'settings',
    component: Settings,
    data: { [PAGE_TITLE_KEY]: 'pageTitle.settings' },
  },
  {
    path: 'filters',
    component: ProductsContainer
  },
  {
    path: 'favorites',
    component: Favorites,
    canActivate: [AuthGuard],
  },
  {
    path: 'products',
    component: ProductsList
  },
  { 
    path: 'product/:id', 
    component: ProductDetails
   },
   {
    path: 'product-proposal/:id',
    component: ProductProposal,
    canActivate: [AuthGuard]
   },
   {
    path: 'criterion-proposal/:id',
    component: CriterionProposal,
    canActivate: [AuthGuard]
   },
  { 
    path: 'reset-password', 
    component: ResetPassword
  },
  { 
    path: 'confirm-email', 
    component: EmailConfirmation
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
