import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/asset-list/asset-list').then((m) => m.AssetList),
    canActivate: [authGuard]
  },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then((m) => m.Login),
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    loadComponent: () => import('./features/register/register').then((m) => m.Register),
    canActivate: [guestGuard]
  },
  { path: '**', redirectTo: '' }
];
