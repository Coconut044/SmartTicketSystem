import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { 
    path: '', 
    redirectTo: '/login',
    pathMatch: 'full' 
  },
  {
    path: 'login',
    loadComponent: () => import('./pages/login/login.page').then(m => m.LoginPage)
  },
  {
    path: 'register',
    loadComponent: () => import('./pages/register/register.page').then(m => m.RegisterPage)
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.page').then(m => m.DashboardPage),
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin', 'SupportManager'] }
  },
  {
    path: 'tickets',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () => import('./pages/ticket-list/ticket-list.page').then(m => m.TicketListPage),
        canActivate: [RoleGuard],
        data: { roles: ['Admin', 'SupportManager'] }
      },
      {
        path: 'create',
        loadComponent: () => import('./pages/ticket-create/ticket-create.page').then(m => m.TicketCreatePage),
        canActivate: [RoleGuard],
        data: { roles: ['EndUser', 'Customer'] }
      },
      {
        path: ':id',
        loadComponent: () => import('./pages/ticket-view/ticket-view.page').then(m => m.TicketViewPage)
        // ✅✅✅ NO ROLE GUARD HERE - EVERYONE SHOULD ACCESS THIS ✅✅✅
      }
    ]
  },
  {
    path: 'my-tickets',
    loadComponent: () => import('./pages/my-tickets/my-tickets.page').then(m => m.MyTicketsPage),
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['EndUser', 'Customer'] }
  },
  {
    path: 'queue',
    loadComponent: () => import('./pages/ticket-queue/ticket-queue.page').then(m => m.TicketQueuePage),
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin', 'SupportManager', 'SupportAgent'] }
  },
  {
    path: 'admin',
    loadComponent: () => import('./pages/admin/admin.page').then(m => m.AdminPage),
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin'] }
  },
  {
    path: 'reports',
    loadComponent: () => import('./pages/reports/reports.page').then(m => m.ReportsPage),
    canActivate: [authGuard, RoleGuard],
    data: { roles: ['Admin', 'SupportManager'] }
  },
  { 
    path: '**', 
    redirectTo: '/login'
  }
];