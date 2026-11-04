import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { environment } from '../../../environments/environment';

type ReportTab = 'overview' | 'orders' | 'production' | 'products' | 'users';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, MatButtonModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent {
  private readonly http = inject(HttpClient);

  tab: ReportTab = 'overview';
  from = this.dateOffset(30);
  to = this.dateOffset(0);
  loading = false;
  error = '';
  data: any = null;

  constructor() { this.load(); }

  setTab(tab: ReportTab): void {
    this.tab = tab;
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    const params = new URLSearchParams();
    if (this.from) params.set('from', this.from);
    if (this.to) params.set('to', this.to);
    const endpoint: Record<ReportTab, string> = {
      overview: 'summary', orders: 'orders', production: 'production',
      products: 'products', users: 'users'
    };
    this.http.get<any>(`${environment.apiUrl}/reports/${endpoint[this.tab]}?${params}`).subscribe({
      next: result => { this.data = result; this.loading = false; },
      error: error => {
        this.error = error.error?.detail || 'گزارش دریافت نشد. دسترسی یا ارتباط با سرور را بررسی کنید.';
        this.loading = false;
      }
    });
  }

  statusText(status: string): string {
    return ({ Draft: 'پیش‌نویس', Active: 'در حال انجام', Completed: 'تکمیل‌شده',
      HasShortage: 'دارای کسری', Cancelled: 'لغوشده' } as Record<string, string>)[status] ?? status;
  }

  resultText(result: string): string {
    return ({ Approved: 'تأیید', Rejected: 'رد شده' } as Record<string, string>)[result] ?? result;
  }

  number(value: number | null | undefined): string {
    return new Intl.NumberFormat('fa-IR', { maximumFractionDigits: 0 }).format(value ?? 0);
  }

  duration(minutes: number | null | undefined): string {
    const totalMinutes = Math.max(0, Math.round(minutes ?? 0));
    const hours = Math.floor(totalMinutes / 60);
    const remainingMinutes = totalMinutes % 60;
    if (hours === 0) return `${this.number(remainingMinutes)} دقیقه`;
    if (remainingMinutes === 0) return `${this.number(hours)} ساعت`;
    return `${this.number(hours)} ساعت و ${this.number(remainingMinutes)} دقیقه`;
  }

  date(value: string): string {
    return value ? new Intl.DateTimeFormat('fa-IR').format(new Date(value)) : '—';
  }

  private dateOffset(days: number): string {
    const value = new Date();
    value.setDate(value.getDate() - days);
    return value.toISOString().slice(0, 10);
  }
}
