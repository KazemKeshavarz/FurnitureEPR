import { Component, inject } from '@angular/core';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatSidenavModule, MatToolbarModule, MatListModule,
    MatIconModule, MatButtonModule, MatDividerModule
  ],
  template: `
    <mat-sidenav-container class="shell">
      <mat-sidenav
        #drawer
        class="sidenav"
        [mode]="isMobile ? 'over' : 'side'"
        [opened]="!isMobile">

        <div class="brand">
          <div class="brand-mark">EPR</div>
          <div>
            <strong>Furniture EPR</strong>
            <small>مدیریت سفارش و تولید</small>
          </div>
        </div>

        <mat-divider />

        <mat-nav-list class="menu">
          <a mat-list-item routerLink="/dashboard" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>dashboard</mat-icon>
            <span matListItemTitle>داشبورد</span>
          </a>

          <div class="section-title">عملیات</div>

          <a mat-list-item routerLink="/orders" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>receipt_long</mat-icon>
            <span matListItemTitle>سفارش‌ها</span>
          </a>

          <a mat-list-item routerLink="/production" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>precision_manufacturing</mat-icon>
            <span matListItemTitle>تولید و مراحل کار</span>
          </a>

          <a mat-list-item routerLink="/customers" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>مشتریان</span>
          </a>

          <div class="section-title">تعریف اطلاعات</div>

          <a mat-list-item routerLink="/products" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>chair</mat-icon>
            <span matListItemTitle>محصولات</span>
          </a>

          <a mat-list-item routerLink="/categories" routerLinkActive="active" (click)="closeOnMobile(drawer)">
            <mat-icon matListItemIcon>category</mat-icon>
            <span matListItemTitle>دسته‌بندی‌ها</span>
          </a>

          @if (canManageUsers) {
            <div class="section-title">مدیریت سامانه</div>

            <a mat-list-item routerLink="/admin" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}" (click)="closeOnMobile(drawer)">
              <mat-icon matListItemIcon>settings</mat-icon>
              <span matListItemTitle>مدیریت و تعاریف</span>
            </a>

            <a mat-list-item routerLink="/admin/master-data" routerLinkActive="active" (click)="closeOnMobile(drawer)">
              <mat-icon matListItemIcon>category</mat-icon>
              <span matListItemTitle>تعاریف محصول</span>
            </a>

            <a mat-list-item routerLink="/admin/workflows" routerLinkActive="active" (click)="closeOnMobile(drawer)">
              <mat-icon matListItemIcon>account_tree</mat-icon>
              <span matListItemTitle>فرآیندهای تولید</span>
            </a>

            <a mat-list-item routerLink="/admin/users" routerLinkActive="active" (click)="closeOnMobile(drawer)">
              <mat-icon matListItemIcon>manage_accounts</mat-icon>
              <span matListItemTitle>کاربران</span>
            </a>

            <a mat-list-item routerLink="/admin/roles" routerLinkActive="active" (click)="closeOnMobile(drawer)">
              <mat-icon matListItemIcon>admin_panel_settings</mat-icon>
              <span matListItemTitle>نقش‌ها و دسترسی‌ها</span>
            </a>
          }
        </mat-nav-list>

        <div class="sidenav-footer">
          <div class="user-box">
            <div class="avatar">{{ userInitial }}</div>
            <div class="user-info">
              <strong>{{ userName }}</strong>
              <small>کاربر سامانه</small>
            </div>
          </div>
          <button mat-button class="logout-button" (click)="logout()">
            <mat-icon>logout</mat-icon>
            خروج از سامانه
          </button>
        </div>
      </mat-sidenav>

      <mat-sidenav-content>
        <mat-toolbar class="topbar">
          <button mat-icon-button class="menu-button" aria-label="باز کردن منو" (click)="drawer.toggle()">
            <mat-icon>menu</mat-icon>
          </button>
          <div class="page-title">
            <strong>Furniture EPR</strong>
            <span>سامانه مدیریت سفارش و تولید</span>
          </div>
          <span class="toolbar-spacer"></span>
          <button mat-icon-button class="desktop-logout" aria-label="خروج" (click)="logout()">
            <mat-icon>logout</mat-icon>
          </button>
        </mat-toolbar>

        <main class="content">
          <router-outlet />
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
  styles: [`
    .shell { height: 100vh; background: #f5f7f8; }
    .sidenav { width: 285px; border-left: 1px solid #e5eaec; border-right: 0; background: #fff; }
    .brand { min-height: 82px; padding: 18px 20px; display: flex; align-items: center; gap: 12px; }
    .brand-mark { width: 44px; height: 44px; border-radius: 13px; display: grid; place-items: center; background: #102a35; color: #fff; font-size: 12px; font-weight: 800; }
    .brand strong, .brand small { display: block; }
    .brand strong { color: #102a35; font-size: 15px; }
    .brand small { margin-top: 4px; color: #77858b; font-size: 11px; }
    .menu { padding: 10px 10px; }
    .menu a { min-height: 50px; margin: 3px 0; border-radius: 12px; color: #4e5d63; }
    .menu a.active { background: #eaf3f6; color: #286f93; font-weight: 700; }
    .menu a mat-icon { color: inherit; }
    .section-title { padding: 18px 14px 6px; color: #9aa6ab; font-size: 11px; font-weight: 700; }
    .sidenav-footer { position: absolute; bottom: 0; right: 0; left: 0; padding: 12px; background: #fff; border-top: 1px solid #edf0f1; }
    .user-box { display: flex; align-items: center; gap: 10px; padding: 8px; }
    .avatar { width: 38px; height: 38px; border-radius: 50%; display: grid; place-items: center; background: #286f93; color: #fff; font-weight: 700; }
    .user-info { min-width: 0; }
    .user-info strong, .user-info small { display: block; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .user-info strong { font-size: 13px; color: #102a35; }
    .user-info small { margin-top: 2px; color: #8a969b; font-size: 10px; }
    .logout-button { width: 100%; justify-content: flex-start; color: #9b4c4c; margin-top: 4px; }
    .topbar { position: sticky; top: 0; z-index: 10; height: 64px; background: rgba(255,255,255,.96); border-bottom: 1px solid #e7ecee; box-shadow: none; }
    .menu-button { display: none; }
    .page-title strong, .page-title span { display: block; }
    .page-title strong { color: #102a35; font-size: 14px; }
    .page-title span { color: #849197; font-size: 10px; margin-top: 2px; }
    .toolbar-spacer { flex: 1; }
    .content { min-height: calc(100vh - 64px); padding: 20px; }
    @media (max-width: 800px) {
      .sidenav { width: min(86vw, 310px); }
      .menu-button { display: inline-flex; margin-left: 6px; }
      .desktop-logout { display: none; }
      .topbar { height: 58px; padding: 0 8px; }
      .content { padding: 12px; min-height: calc(100vh - 58px); }
      .page-title span { display: none; }
    }
  `
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpoint = inject(BreakpointObserver);

  isMobile = false;

  constructor() {
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
    return this.auth.hasPermission('identity.manage_users') || this.auth.hasPermission('identity.manage_roles');
  }

  closeOnMobile(drawer: { close: () => void }): void {
    if (this.isMobile) drawer.close();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
