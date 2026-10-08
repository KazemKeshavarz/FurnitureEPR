import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { permissionGuard } from './core/auth/permission.guard';

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
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'سفارش‌ها', description: 'بخش ثبت و پیگیری سفارش‌ها در حال آماده‌سازی است.' }
      },
      {
        path: 'production',
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'تولید و مراحل کار', description: 'بخش اجرای مراحل تولید و کنترل کیفیت در حال آماده‌سازی است.' }
      },
      {
        path: 'customers',
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'مشتریان', description: 'بخش مدیریت مشتریان در حال آماده‌سازی است.' }
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
        path: 'admin/users',
        canActivate: [permissionGuard('identity.manage_users')],
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'کاربران', description: 'مدیریت کاربران سامانه در مرحله بعدی تکمیل می‌شود.' }
      },
      {
        path: 'admin/roles',
        canActivate: [permissionGuard('identity.manage_roles')],
        loadComponent: () => import('./features/placeholder/placeholder.component').then(m => m.PlaceholderComponent),
        data: { title: 'نقش‌ها و دسترسی‌ها', description: 'مدیریت نقش‌ها و دسترسی‌ها در مرحله بعدی تکمیل می‌شود.' }
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
