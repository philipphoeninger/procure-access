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
import { Unauthorized } from './pages/unauthorized/unauthorized';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'home',
  },
  {
    path: 'home',
    component: Home
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
  },
  {
    path: 'settings',
    component: Settings
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
    path: 'product-proposal',
    component: ProductProposal,
    canActivate: [AuthGuard]
   },
   {
    path: 'criterion-proposal',
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
  { 
    path: 'unauthorized', 
    component: Unauthorized
  }
];
