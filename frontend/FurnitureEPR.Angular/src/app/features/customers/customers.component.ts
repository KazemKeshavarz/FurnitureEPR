import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';

interface Customer {
  id: string;
  name: string;
  phoneNumber?: string | null;
  address?: string | null;
}

interface CustomerPage {
  items?: Customer[];
  data?: Customer[];
  totalCount?: number;
  total?: number;
}

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule
  ],
  template: `
    <section class="page" dir="rtl">
      <header class="page-heading">
        <div>
          <span class="eyebrow">اطلاعات پایه</span>
          <h1>مشتریان</h1>
          <p>اطلاعات تماس مشتریان سفارش‌ها را مدیریت کنید.</p>
        </div>
        <button mat-flat-button class="new-button" (click)="toggleForm()">
          <mat-icon>{{ showForm ? 'close' : 'person_add' }}</mat-icon>
          {{ showForm ? 'بستن فرم' : 'مشتری جدید' }}
        </button>
      </header>

      @if (showForm) {
        <mat-card class="form-card">
          <h2>ثبت مشتری جدید</h2>
          <form [formGroup]="form" (ngSubmit)="createCustomer()">
            <mat-form-field appearance="outline">
              <mat-label>نام و نام خانوادگی / نام شرکت</mat-label>
              <input matInput formControlName="name" autocomplete="name" placeholder="مثلاً علی رضایی">
              @if (form.controls.name.hasError('required') && form.controls.name.touched) {
                <mat-error>نام مشتری الزامی است.</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>شماره تماس</mat-label>
              <input matInput formControlName="phoneNumber" inputmode="tel" autocomplete="tel" placeholder="09...">
            </mat-form-field>
            <mat-form-field appearance="outline">
              <mat-label>آدرس</mat-label>
              <textarea matInput formControlName="address" rows="2" placeholder="آدرس مشتری (اختیاری)"></textarea>
            </mat-form-field>
            <button mat-flat-button class="save-button" type="submit" [disabled]="form.invalid || saving">
              {{ saving ? 'در حال ثبت...' : 'ثبت مشتری' }}
            </button>
          </form>
        </mat-card>
      }

      <div class="search-row">
        <mat-form-field appearance="outline" class="search-field">
          <mat-label>جست‌وجوی مشتری</mat-label>
          <input matInput [value]="search" (input)="onSearch($any($event.target).value)" placeholder="نام یا شماره تماس">
          <mat-icon matPrefix>search</mat-icon>
        </mat-form-field>
        <button mat-stroked-button class="refresh-button" (click)="loadCustomers()">
          <mat-icon>refresh</mat-icon><span>بروزرسانی</span>
        </button>
      </div>

      @if (error) {
        <div class="error-message" role="alert">{{ error }}</div>
      }
      @if (loading) {
        <div class="state-card"><mat-icon>hourglass_top</mat-icon><span>در حال دریافت مشتریان...</span></div>
      } @else if (customers.length === 0) {
        <div class="state-card empty">
          <mat-icon>group</mat-icon>
          <strong>{{ search ? 'مشتری‌ای با این مشخصات پیدا نشد.' : 'هنوز مشتری ثبت نشده است.' }}</strong>
          <span>برای شروع، مشتری جدید را ثبت کنید.</span>
        </div>
      } @else {
        <div class="customer-list">
          @for (customer of customers; track customer.id) {
            <mat-card class="customer-card">
              <div class="avatar"><mat-icon>person</mat-icon></div>
              <div class="customer-info">
                <strong>{{ customer.name }}</strong>
                <span class="phone" dir="ltr">{{ customer.phoneNumber || 'شماره تماس ثبت نشده' }}</span>
                @if (customer.address) { <span class="address">{{ customer.address }}</span> }
              </div>
              <button mat-icon-button aria-label="نمایش اطلاعات مشتری" [disabled]="!customer.address" (click)="showAddress(customer)">
                <mat-icon>location_on</mat-icon>
              </button>
            </mat-card>
          }
        </div>
        <div class="list-footer">نمایش {{ customers.length }} مشتری</div>
      }
    </section>
  `,
  styles: [`
    .page{max-width:900px;margin:0 auto;color:#102a35}.page-heading{display:flex;align-items:center;justify-content:space-between;gap:14px;margin-bottom:20px}.eyebrow{font-size:12px;color:#286f93;font-weight:700}.page-heading h1{font-size:25px;margin:5px 0}.page-heading p{margin:0;color:#718087;font-size:13px}.new-button,.save-button{background:#286f93!important;color:#fff!important;min-height:48px;border-radius:12px}.new-button mat-icon{margin-left:5px}.form-card{padding:18px;border-radius:18px;margin-bottom:18px}.form-card h2{font-size:17px;margin:0 0 14px}form{display:grid;grid-template-columns:1fr 1fr;gap:8px 12px}form mat-form-field:first-child,form mat-form-field:nth-child(3),form button{grid-column:1/-1}form mat-form-field{width:100%}.search-row{display:flex;align-items:start;gap:10px}.search-field{flex:1}.refresh-button{height:56px;border-radius:12px;color:#286f93}.customer-list{display:grid;gap:10px}.customer-card{display:flex;align-items:center;gap:12px;padding:13px;border-radius:16px;box-shadow:0 2px 12px #102a3508}.avatar{width:44px;height:44px;border-radius:14px;background:#e8f3f7;display:grid;place-items:center;color:#286f93;flex-shrink:0}.customer-info{min-width:0;flex:1;display:grid;gap:5px}.customer-info strong{font-size:15px;overflow-wrap:anywhere}.phone,.address{font-size:12px;color:#718087;overflow-wrap:anywhere}.customer-card button{color:#286f93}.state-card{min-height:130px;display:flex;align-items:center;justify-content:center;gap:10px;flex-direction:column;border:1px dashed #d7e0e4;border-radius:16px;color:#718087;padding:20px;text-align:center}.state-card mat-icon{font-size:30px;width:30px;height:30px;color:#286f93}.state-card strong{color:#102a35}.error-message{background:#fff0f0;color:#a33;padding:12px;border-radius:12px;margin-bottom:12px}.list-footer{text-align:center;font-size:12px;color:#839096;padding:15px}.save-button{grid-column:1/-1}@media(max-width:600px){.page-heading{align-items:flex-start;flex-direction:column}.new-button{width:100%}.page-heading h1{font-size:22px}.form-card{padding:14px}form{grid-template-columns:1fr}form mat-form-field,form button{grid-column:1}.search-row{gap:6px}.refresh-button{padding:0 10px}.refresh-button span{display:none}.customer-card{padding:11px}.customer-info strong{font-size:14px}}
  `]
})
export class CustomersComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  customers: Customer[] = [];
  search = '';
  showForm = false;
  loading = false;
  saving = false;
  error = '';
  private searchTimer?: ReturnType<typeof setTimeout>;

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    phoneNumber: ['', [Validators.maxLength(30)]],
    address: ['', [Validators.maxLength(1000)]]
  });

  constructor() { this.loadCustomers(); }

  toggleForm(): void {
    this.showForm = !this.showForm;
    this.error = '';
  }

  onSearch(value: string): void {
    this.search = value;
    if (this.searchTimer) clearTimeout(this.searchTimer);
    this.searchTimer = setTimeout(() => this.loadCustomers(), 300);
  }

  loadCustomers(): void {
    this.loading = true;
    this.error = '';
    const params: Record<string, string> = { page: '1', pageSize: '100' };
    if (this.search.trim()) params['search'] = this.search.trim();

    this.http.get<CustomerPage | Customer[]>(`${environment.apiUrl}/customers`, { params }).subscribe({
      next: response => {
        this.customers = Array.isArray(response)
          ? response
          : response.items ?? response.data ?? [];
        this.loading = false;
      },
      error: () => {
        this.error = 'دریافت فهرست مشتریان انجام نشد. اتصال سامانه را بررسی کنید.';
        this.loading = false;
      }
    });
  }

  createCustomer(): void {
    if (this.form.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();
    this.http.post<string>(`${environment.apiUrl}/customers`, {
      name: value.name.trim(),
      phoneNumber: value.phoneNumber.trim() || null,
      address: value.address.trim() || null
    }).subscribe({
      next: () => {
        this.form.reset({ name: '', phoneNumber: '', address: '' });
        this.showForm = false;
        this.saving = false;
        this.loadCustomers();
      },
      error: response => {
        this.error = response.error?.detail || 'ثبت مشتری انجام نشد.';
        this.saving = false;
      }
    });
  }

  showAddress(customer: Customer): void {
    if (customer.address) {
      this.error = `آدرس ${customer.name}: ${customer.address}`;
    }
  }
}
