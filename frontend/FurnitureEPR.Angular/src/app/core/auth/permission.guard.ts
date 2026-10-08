import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const permissionGuard = (permission: string): CanActivateFn => () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  return authService.hasPermission(permission)
    ? true
    : router.createUrlTree(['/']);
};

export const anyPermissionGuard = (permissions: string[]): CanActivateFn => () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  return permissions.some(permission => authService.hasPermission(permission))
    ? true
    : router.createUrlTree(['/']);
};
