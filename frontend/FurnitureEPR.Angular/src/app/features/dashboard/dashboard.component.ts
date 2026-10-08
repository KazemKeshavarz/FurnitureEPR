import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatButtonModule, MatCardModule],
  template: `
    <main class="dashboard">
      <header><div><span>Furniture EPR</span><h1>داشبورد</h1></div><button mat-stroked-button (click)="logout()">خروج</button></header>
      <section class="welcome"><p>خوش آمدید</p><h2>{{ userName }}</h2><small>سامانه مدیریت سفارش، تولید و کنترل کیفیت</small></section>
      <section class="cards">
        <mat-card><strong>سفارش‌ها</strong><span>مدیریت و پیگیری سفارش‌های مشتریان</span></mat-card>
        <mat-card><strong>فرآیند تولید</strong><span>اجرای مراحل Workflow و کنترل کیفیت</span></mat-card>
        <mat-card><strong>گزارش‌ها</strong><span>گزارش وضعیت سفارش‌ها و تولید</span></mat-card>
      </section>
    </main>
  `,
  styles: [`
    .dashboard{min-height:100vh;padding:32px clamp(18px,5vw,72px);background:#f7f9fa} header{display:flex;justify-content:space-between;align-items:center} header span{color:#286f93;font-weight:700} h1{margin:6px 0 0;color:#102a35}.welcome{margin:32px 0;padding:28px;border-radius:22px;background:#102a35;color:white}.welcome p{margin:0 0 6px;opacity:.7}.welcome h2{margin:0 0 8px;font-size:28px}.welcome small{opacity:.75}.cards{display:grid;grid-template-columns:repeat(3,1fr);gap:18px}mat-card{padding:24px;border-radius:18px}mat-card strong{display:block;color:#102a35;font-size:18px;margin-bottom:10px}mat-card span{color:#69777d;line-height:1.9}@media(max-width:800px){.cards{grid-template-columns:1fr}}
  `]
})
export class DashboardComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  get userName(): string { return this.auth.getUserName(); }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
