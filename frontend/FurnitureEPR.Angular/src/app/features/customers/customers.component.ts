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
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss'
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
