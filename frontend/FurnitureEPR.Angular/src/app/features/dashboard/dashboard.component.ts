import { Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [MatCardModule],
  template: `
    <section class="dashboard">
      <div class="page-heading">
        <span>صفحه اصلی</span>
        <h1>داشبورد</h1>
      </div>

      <section class="welcome">
        <p>خوش آمدید</p>
        <h2>{{ userName }}</h2>
        <small>از اینجا می‌توانید سفارش‌ها و مراحل تولید را به‌سادگی پیگیری کنید.</small>
      </section>

      <section class="cards">
        <mat-card><div class="icon">📋</div><strong>سفارش‌ها</strong><span>ثبت و پیگیری سفارش‌های مشتریان</span></mat-card>
        <mat-card><div class="icon">🏭</div><strong>تولید</strong><span>مشاهده مرحله فعلی هر سفارش</span></mat-card>
        <mat-card><div class="icon">📊</div><strong>گزارش‌ها</strong><span>گزارش وضعیت سفارش‌ها و تولید</span></mat-card>
      </section>
    </section>
  `,
  styles: [`
    .dashboard { max-width: 1200px; margin: 0 auto; }
    .page-heading span { color: #286f93; font-size: 12px; font-weight: 700; }
    .page-heading h1 { margin: 5px 0 0; color: #102a35; font-size: 26px; }
    .welcome { margin: 20px 0; padding: 24px; border-radius: 20px; background: #102a35; color: #fff; }
    .welcome p { margin: 0 0 5px; opacity: .65; font-size: 12px; }
    .welcome h2 { margin: 0 0 8px; font-size: 24px; }
    .welcome small { opacity: .78; line-height: 1.9; }
    .cards { display: grid; grid-template-columns: repeat(3, 1fr); gap: 14px; }
    mat-card { padding: 20px; border-radius: 18px; box-shadow: 0 3px 16px rgba(16,42,53,.05); }
    .icon { font-size: 26px; margin-bottom: 12px; }
    mat-card strong { display: block; color: #102a35; font-size: 17px; margin-bottom: 7px; }
    mat-card span { color: #68777d; line-height: 1.8; font-size: 13px; }
    @media (max-width: 800px) {
      .page-heading h1 { font-size: 22px; }
      .welcome { margin: 16px 0; padding: 20px; border-radius: 17px; }
      .welcome h2 { font-size: 21px; }
      .cards { grid-template-columns: 1fr; gap: 10px; }
      mat-card { padding: 17px; }
    }
  `]
})
export class DashboardComponent {
  private readonly auth = inject(AuthService);
  get userName(): string { return this.auth.getUserName(); }
}
