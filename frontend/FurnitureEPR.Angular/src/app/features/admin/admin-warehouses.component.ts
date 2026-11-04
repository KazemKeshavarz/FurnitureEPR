import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface Warehouse { id: string; name: string; code: string; isActive: boolean; }

@Component({
  selector: 'app-admin-warehouses',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './admin-warehouses.component.html',
  styleUrl: './admin-warehouses.component.scss'
})
export class AdminWarehousesComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  warehouses: Warehouse[] = [];
  editingId = '';
  busy = false;
  error = '';
  notice = '';

  form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]],
    code: ['', [Validators.required, Validators.maxLength(50)]]
  });

  constructor() { this.load(); }

  load(): void {
    this.http.get<Warehouse[]>(`${environment.apiUrl}/warehouses`).subscribe({
      next: items => this.warehouses = items,
      error: error => this.error = error.error?.detail || 'دریافت فهرست انبارها انجام نشد.'
    });
  }

  edit(item: Warehouse): void {
    this.editingId = item.id;
    this.form.setValue({ name: item.name, code: item.code });
    this.error = '';
    this.notice = 'اطلاعات انبار را ویرایش کنید.';
  }

  cancelEdit(): void {
    this.editingId = '';
    this.form.reset({ name: '', code: '' });
    this.notice = '';
  }

  save(): void {
    if (this.form.invalid || this.busy) return;
    this.busy = true;
    this.error = '';
    const body = this.form.getRawValue();
    const request = this.editingId
      ? this.http.put(`${environment.apiUrl}/warehouses/${this.editingId}`, body)
      : this.http.post(`${environment.apiUrl}/warehouses`, body);
    request.subscribe({
      next: () => {
        this.busy = false;
        this.notice = this.editingId ? 'اطلاعات انبار به‌روزرسانی شد.' : 'انبار جدید ثبت شد.';
        this.cancelEdit();
        this.load();
      },
      error: error => {
        this.busy = false;
        this.error = error.error?.detail || error.error || 'ذخیره انبار انجام نشد.';
      }
    });
  }

  toggle(item: Warehouse): void {
    if (this.busy) return;
    this.busy = true;
    this.http.patch(`${environment.apiUrl}/warehouses/${item.id}/active`, !item.isActive).subscribe({
      next: () => {
        item.isActive = !item.isActive;
        this.busy = false;
      },
      error: error => {
        this.busy = false;
        this.error = error.error?.detail || 'تغییر وضعیت انبار انجام نشد.';
      }
    });
  }
}
