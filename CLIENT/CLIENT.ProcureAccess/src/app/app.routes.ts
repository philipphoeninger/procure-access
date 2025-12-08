import { Routes } from '@angular/router';
import { Login } from '@features/identity/login/login';
import { Register } from '@features/identity/register/register';
import { Home } from '@pages/home/home';
import { AuthGuard } from '@core/guards/auth.guard';
import { Profile } from '@features/identity/profile/profile';
import { Settings } from '@features/settings/settings';
import { ProductsContainer } from '@features/products/pages/products-container/products-container';
import { Favorites } from '@app/features/favorites/list/favorites';
import { ProductsList } from './features/products/list/products-list';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'home',
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
    path: 'home',
    component: Home,
    canActivate: [AuthGuard],
  },
  {
    path: 'profile',
    component: Profile,
    canActivate: [AuthGuard],
  },
  {
    path: 'settings',
    component: Settings,
    canActivate: [AuthGuard],
  },
  {
    path: 'filters',
    component: ProductsContainer,
    canActivate: [AuthGuard],
  },
  {
    path: 'favorites',
    component: Favorites,
    canActivate: [AuthGuard],
  },
  {
    path: 'products',
    component: ProductsList,
    canActivate: [AuthGuard],
  },
];
