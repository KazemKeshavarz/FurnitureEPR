import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { environment } from '../../../environments/environment';

interface OrderListItem {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName: string;
  status: string;
  createdAtUtc: string;
  finalizedAtUtc?: string | null;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  itemsCount: number;
}

interface PagedOrders {
  items: OrderListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
}

@Component({
  selector: 'app-orders-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterLink,
    MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatSelectModule
  ],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <span class="eyebrow">عملیات فروش</span>
          <h1>سفارش‌ها</h1>
          <p>سفارش‌ها را جستجو و وضعیت آن‌ها را پیگیری کنید.</p>
        </div>
        <a mat-flat-button class="new-button" routerLink="/orders/new">
          <mat-icon>add</mat-icon>
          سفارش جدید
        </a>
      </header>

      <div class="filters">
        <mat-form-field appearance="outline" class="search">
          <mat-label>جستجوی سفارش یا مشتری</mat-label>
          <input matInput [(ngModel)]="search" (keyup.enter)="load()" placeholder="مثلاً 10025 یا نام مشتری">
          @if(search){<button mat-icon-button matSuffix type="button" aria-label="پاک کردن" (click)="search='';load()"><mat-icon>close</mat-icon></button>}
        </mat-form-field>

        <mat-form-field appearance="outline" class="status">
          <mat-label>وضعیت</mat-label>
          <mat-select [(ngModel)]="status" (selectionChange)="load()">
            <mat-option value="">همه سفارش‌ها</mat-option>
            <mat-option value="Draft">پیش‌نویس</mat-option>
            <mat-option value="Active">در حال انجام</mat-option>
            <mat-option value="Completed">تکمیل‌شده</mat-option>
            <mat-option value="Cancelled">لغوشده</mat-option>
          </mat-select>
        </mat-form-field>
      </div>

      @if(loading){
        <div class="state"><mat-icon>sync</mat-icon><span>در حال دریافت سفارش‌ها...</span></div>
      } @else if(error) {
        <div class="error">{{error}} <button mat-button type="button" (click)="load()">تلاش مجدد</button></div>
      } @else if(!orders.length) {
        <div class="empty">
          <mat-icon>receipt_long</mat-icon>
          <strong>سفارشی پیدا نشد</strong>
          <span>برای شروع، یک سفارش جدید ثبت کنید.</span>
          <a mat-flat-button routerLink="/orders/new">ثبت سفارش</a>
        </div>
      } @else {
        <div class="order-list">
          @for(order of orders; track order.id){
            <button class="order-card" type="button" (click)="open(order.id)">
              <div class="card-top">
                <div>
                  <strong>سفارش {{order.orderNumber}}</strong>
                  <small>{{order.customerName}}</small>
                </div>
                <span class="status-badge" [class]="statusClass(order.status)">{{statusText(order.status)}}</span>
              </div>
              <div class="card-middle">
                <span><mat-icon>inventory_2</mat-icon>{{order.itemsCount}} قلم</span>
                <span><mat-icon>calendar_today</mat-icon>{{formatDate(order.createdAtUtc)}}</span>
              </div>
              <div class="card-bottom">
                <span>مبلغ نهایی</span>
                <strong>{{format(order.finalAmount)}} ریال</strong>
                <mat-icon>chevron_left</mat-icon>
              </div>
            </button>
          }
        </div>

        <div class="pager">
          <button mat-stroked-button type="button" [disabled]="page<=1" (click)="changePage(page-1)">
            <mat-icon>chevron_right</mat-icon> قبلی
          </button>
          <span>صفحه {{page}} از {{totalPages}}</span>
          <button mat-stroked-button type="button" [disabled]="page>=totalPages" (click)="changePage(page+1)">
            بعدی <mat-icon>chevron_left</mat-icon>
          </button>
        </div>
      }
    </section>
  `,
  styles: [`
    .page{max-width:900px;margin:0 auto}.page-header{display:flex;align-items:center;justify-content:space-between;gap:14px;margin-bottom:18px}.eyebrow{font-size:11px;color:#286f93;font-weight:800}.page-header h1{margin:4px 0;color:#102a35;font-size:25px}.page-header p{margin:0;color:#7c898e;font-size:12px}.new-button{height:48px;background:#286f93;color:#fff;border-radius:12px;white-space:nowrap}.filters{display:grid;grid-template-columns:1fr 190px;gap:10px;margin-bottom:12px}.search,.status{width:100%}.order-list{display:grid;gap:9px}.order-card{width:100%;text-align:right;border:1px solid #e4eaec;background:#fff;border-radius:16px;padding:14px;cursor:pointer;box-shadow:0 2px 8px rgba(16,42,53,.03)}.card-top,.card-bottom{display:flex;align-items:center;gap:8px}.card-top>div{flex:1;min-width:0}.card-top strong,.card-top small{display:block;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}.card-top strong{color:#102a35;font-size:14px}.card-top small{margin-top:4px;color:#7c898e;font-size:11px}.status-badge{padding:5px 9px;border-radius:20px;font-size:10px;font-weight:800;white-space:nowrap}.draft{background:#f2f3f4;color:#68757b}.active{background:#e9f4f8;color:#286f93}.completed{background:#eaf6ef;color:#28734a}.cancelled{background:#fff0f0;color:#a14f4f}.card-middle{display:flex;gap:18px;padding:11px 0;border-bottom:1px solid #eef1f2;color:#7d8a8f;font-size:10px}.card-middle span{display:flex;align-items:center;gap:4px}.card-middle mat-icon{font-size:15px;width:15px;height:15px}.card-bottom{padding-top:10px}.card-bottom span{color:#879398;font-size:10px}.card-bottom strong{margin-right:auto;color:#286f93;font-size:14px}.card-bottom mat-icon{color:#a2adb1}.pager{display:flex;align-items:center;justify-content:center;gap:12px;padding:16px 0}.pager span{font-size:11px;color:#6f7d82;min-width:80px;text-align:center}.state,.empty{text-align:center;background:#fff;border:1px dashed #dce5e8;border-radius:16px;padding:34px 15px;color:#829095}.state{display:flex;justify-content:center;align-items:center;gap:8px}.empty{display:grid;justify-items:center;gap:7px}.empty mat-icon{font-size:40px;width:40px;height:40px;color:#286f93}.empty strong{color:#102a35}.empty span{font-size:11px;margin-bottom:8px}.empty a{background:#286f93;color:#fff}.error{padding:13px;border-radius:13px;background:#fff0f0;color:#9d4c4c;font-size:12px;text-align:center}.error button{color:#286f93}@media(max-width:650px){.page-header{align-items:stretch;flex-direction:column}.new-button{width:100%;justify-content:center}.filters{grid-template-columns:1fr}.page-header h1{font-size:21px}.order-card{padding:13px}.pager button{min-height:46px}.pager{gap:7px}}
  `]
})
export class OrdersListComponent {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  orders: OrderListItem[] = [];
  search = '';
  status = '';
  page = 1;
  readonly pageSize = 15;
  totalCount = 0;
  loading = false;
  error = '';

  constructor() { this.load(); }

  get totalPages(): number { return Math.max(1, Math.ceil(this.totalCount / this.pageSize)); }

  load(): void {
    this.loading = true;
    this.error = '';
    const params = new URLSearchParams({
      page: String(this.page),
      pageSize: String(this.pageSize)
    });
    if (this.search.trim()) params.set('search', this.search.trim());
    if (this.status) params.set('status', this.status);

    this.http.get<PagedOrders>(`${environment.apiUrl}/orders?${params}`).subscribe({
      next: result => {
        this.orders = result.items ?? [];
        this.totalCount = result.totalCount ?? 0;
        this.loading = false;
      },
      error: e => {
        this.error = e.error?.detail || 'دریافت سفارش‌ها انجام نشد.';
        this.loading = false;
      }
    });
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.page = page;
    this.load();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  open(id: string): void { this.router.navigate(['/orders', id]); }

  statusText(status: string): string {
    return ({Draft:'پیش‌نویس',Active:'در حال انجام',Completed:'تکمیل‌شده',Cancelled:'لغوشده'} as Record<string,string>)[status] ?? status;
  }

  statusClass(status: string): string {
    return ({Draft:'draft',Active:'active',Completed:'completed',Cancelled:'cancelled'} as Record<string,string>)[status] ?? 'draft';
  }

  format(value: number): string { return new Intl.NumberFormat('fa-IR').format(Math.round(value)); }
  formatDate(value: string): string { return new Intl.DateTimeFormat('fa-IR',{year:'numeric',month:'2-digit',day:'2-digit'}).format(new Date(value)); }
}
