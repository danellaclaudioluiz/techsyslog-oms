import { Routes } from '@angular/router';
import { authGuard, guestGuard, operatorGuard, adminGuard } from '@core/guards';

export const routes: Routes = [
  // Redirect root to dashboard or login
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },

  // Auth routes (guest only)
  {
    path: 'auth',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('@shared/components/layout').then(m => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('@features/auth/pages/login/login-page.component').then(
            m => m.LoginPageComponent
          ),
        title: 'Login - TechsysLog',
      },
      {
        path: 'register',
        loadComponent: () =>
          import('@features/auth/pages/register/register-page.component').then(
            m => m.RegisterPageComponent
          ),
        title: 'Criar Conta - TechsysLog',
      },
    ],
  },

  // Protected routes (authenticated only)
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () =>
      import('@shared/components/layout').then(m => m.MainLayoutComponent),
    children: [
      // Dashboard
      {
        path: 'dashboard',
        loadComponent: () =>
          import('@features/dashboard/pages/dashboard/dashboard-page.component').then(
            m => m.DashboardPageComponent
          ),
        title: 'Dashboard - TechsysLog',
      },

      // Orders
      {
        path: 'orders',
        children: [
          {
            path: '',
            loadComponent: () =>
              import('@features/orders/pages/orders-list/orders-list-page.component').then(
                m => m.OrdersListPageComponent
              ),
            title: 'Pedidos - TechsysLog',
          },
          {
            path: 'new',
            // canActivate: [operatorGuard],
            loadComponent: () =>
              import('@features/orders/pages/order-form/order-form-page.component').then(
                m => m.OrderFormPageComponent
              ),
            title: 'Novo Pedido - TechsysLog',
          },
          {
            path: ':id',
            loadComponent: () =>
              import('@features/orders/pages/order-detail/order-detail-page.component').then(
                m => m.OrderDetailPageComponent
              ),
            title: 'Detalhes do Pedido - TechsysLog',
          },
        ],
      },

      {
        path: 'notifications',
        loadComponent: () =>
          import('@features/notifications/pages/notifications-list/notifications-list-page.component').then(
            m => m.NotificationsListPageComponent
          ),
        title: 'Notificações - TechsysLog',
      },

      // Deliveries
      {
        path: 'deliveries',
        children: [
          {
            path: '',
            loadComponent: () =>
              import('@features/deliveries/pages/deliveries-list/deliveries-list-page.component').then(
                m => m.DeliveriesListPageComponent
              ),
            title: 'Entregas - TechsysLog',
          },
        ],
      },

      // Users (Admin only)
      {
        path: 'users',
        canActivate: [adminGuard],
        children: [
          {
            path: '',
            loadComponent: () =>
              import('@features/orders/pages/orders-list/orders-list-page.component').then(
                m => m.OrdersListPageComponent
              ),
            title: 'Usuários - TechsysLog',
          },
        ],
      },
    ],
  },

  // 404
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
