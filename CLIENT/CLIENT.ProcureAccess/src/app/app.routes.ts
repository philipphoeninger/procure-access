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
import { ProductProposition } from '@app/features/products/proposition/product-proposition';
import { CriterionProposition } from '@app/features/criteria/proposition/criterion-proposition';

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
    path: 'product-proposition',
    component: ProductProposition,
    canActivate: [AuthGuard]
   },
   {
    path: 'criterion-proposition',
    component: CriterionProposition,
    canActivate: [AuthGuard]
   }
];
