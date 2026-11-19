import { Component, inject } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../auth/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule,
    MatIconModule, MatButtonModule, MatDividerModule
  ],
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpoint = inject(BreakpointObserver);

  isMobile = false;
  private currentUrl = '';
  private previousUrl = '';

  constructor() {
    this.currentUrl = this.router.url;
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe(event => {
      const nextUrl = (event as NavigationEnd).urlAfterRedirects;
      if (nextUrl !== this.currentUrl) {
        this.previousUrl = this.currentUrl;
        this.currentUrl = nextUrl;
      }
    });
    this.breakpoint.observe(['(max-width: 800px)']).subscribe(state => {
      this.isMobile = state.matches;
    });
  }

  get userName(): string {
    return this.auth.getUserName();
  }

  get userInitial(): string {
    return this.userName.trim().charAt(0) || 'ک';
  }

  get canManageUsers(): boolean {
    return this.auth.hasPermission('identity.manage_users');
  }

  get canManageRoles(): boolean {
    return this.auth.hasPermission('identity.manage_roles');
  }

  get canManageIdentity(): boolean {
    return this.canManageUsers || this.canManageRoles;
  }

  get canViewOrders(): boolean {
    return this.auth.hasPermission('orders.view_all');
  }

  get canViewProduction(): boolean {
    return ['workflow.view_tasks', 'workflow.move', 'workflow.quality_control', 'workflow.complete', 'workflow.manage']
      .some(permission => this.auth.hasPermission(permission));
  }

  get canViewCustomers(): boolean {
    return this.auth.hasPermission('orders.view_all') || this.auth.hasPermission('orders.create');
  }

  get canRequestTransfers(): boolean {
    return this.auth.hasPermission('workflow.move') || this.auth.hasPermission('workflow.manage');
  }

  get canManageWorkflows(): boolean {
    return this.auth.hasPermission('workflow.manage');
  }

  get showBackButton(): boolean {
    return this.currentUrl !== '/dashboard' && this.currentUrl !== '/login' && this.currentUrl !== '/';
  }

  goBack(): void {
    if (this.previousUrl && this.previousUrl !== '/login') {
      this.router.navigateByUrl(this.previousUrl);
      return;
    }

    const path = this.currentUrl.split('?')[0];
    if (/^\/orders\/[^/]+\/(edit)?$/.test(path) || /^\/orders\/[^/]+$/.test(path)) {
      this.router.navigate(['/orders']);
    } else if (path.startsWith('/admin/')) {
      this.router.navigate(['/admin']);
    } else {
      this.router.navigate(['/dashboard']);
    }
  }

  closeOnMobile(drawer: { close: () => void }): void {
    if (this.isMobile) drawer.close();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
