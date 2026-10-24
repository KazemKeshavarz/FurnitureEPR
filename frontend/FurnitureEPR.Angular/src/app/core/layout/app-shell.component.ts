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
  templateUrl: './app-shell.component.html',
  styleUrl: './app-shell.component.scss'
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

  get canManageWorkflows(): boolean {
    return this.auth.hasPermission('workflow.manage');
  }

  closeOnMobile(drawer: { close: () => void }): void {
    if (this.isMobile) drawer.close();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
