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
  templateUrl: './orders-list.component.html',
  styleUrl: './orders-list.component.scss'
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
