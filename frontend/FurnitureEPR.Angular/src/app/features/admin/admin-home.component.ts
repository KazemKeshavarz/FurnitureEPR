import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-admin-home',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatIconModule],
  templateUrl: './admin-home.component.html',
  styleUrl: './admin-home.component.scss'
})
export class AdminHomeComponent {
  private readonly auth = inject(AuthService);

  get canManageMasterData(): boolean {
    return this.auth.hasPermission('identity.manage_users')
      || this.auth.hasPermission('identity.manage_roles');
  }

  get canManageWorkflows(): boolean {
    return this.auth.hasPermission('workflow.manage');
  }

  get canManageUsers(): boolean {
    return this.auth.hasPermission('identity.manage_users');
  }

  get canManageRoles(): boolean {
    return this.auth.hasPermission('identity.manage_roles');
  }
}