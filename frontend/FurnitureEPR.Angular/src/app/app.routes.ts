import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { anyPermissionGuard, permissionGuard } from './core/auth/permission.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./core/layout/app-shell.component').then(m => m.AppShellComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./features/orders/orders-list.component').then(m => m.OrdersListComponent)
      },
      {
        path: 'orders/new',
        loadComponent: () => import('./features/orders/order-create.component').then(m => m.OrderCreateComponent)
      },
      {
        path: 'orders/:id',
        loadComponent: () => import('./features/orders/order-details.component').then(m => m.OrderDetailsComponent)
      },
      {
        path: 'production',
        loadComponent: () => import('./features/production/production.component').then(m => m.ProductionComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/customers/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'products',
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'محصولات', description: 'بخش مدیریت محصولات در حال آماده‌سازی است.' }
      },
      {
        path: 'categories',
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'دسته‌بندی‌ها', description: 'بخش مدیریت دسته‌بندی‌ها در حال آماده‌سازی است.' }
      },
      {
        path: 'admin',
        canActivate: [anyPermissionGuard(['identity.manage_users', 'identity.manage_roles'])],
        loadComponent: () => import('./features/admin/admin-home.component').then(m => m.AdminHomeComponent)
      },
      {
        path: 'admin/master-data',
        canActivate: [anyPermissionGuard(['identity.manage_users', 'identity.manage_roles'])],
        loadComponent: () => import('./features/master-data/master-data.component').then(m => m.MasterDataComponent)
      },
      {
        path: 'admin/workflows',
        canActivate: [permissionGuard('workflow.move')],
        loadComponent: () => import('./features/admin/admin-workflows.component').then(m => m.AdminWorkflowsComponent)
      },
      {
        path: 'admin/users',
        canActivate: [permissionGuard('identity.manage_users')],
        loadComponent: () => import('./features/admin/admin-users.component').then(m => m.AdminUsersComponent)
      },
      {
        path: 'admin/roles',
        canActivate: [permissionGuard('identity.manage_roles')],
        loadComponent: () => import('./features/admin/admin-roles.component').then(m => m.AdminRolesComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
