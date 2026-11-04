import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

interface Category { id: string; name: string; }
interface Role { id: string; name: string; permissions: string[]; }
interface Stage {
  id: string;
  name: string;
  code: string;
  sortOrder: number;
  requiresQualityControl: boolean;
  responsibleRoleId?: string | null;
}
interface Transition { id: string; fromStageId: string; toStageId: string; name: string; }

@Component({
  selector: 'app-admin-workflows',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule, MatInputModule],
  templateUrl: './admin-workflows.component.html',
  styleUrl: './admin-workflows.component.scss'
})
export class AdminWorkflowsComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);

  workflowId = '';
  versionId = '';
  stages: Stage[] = [];
  transitions: Transition[] = [];
  categories: Category[] = [];
  roles: Role[] = [];
  selectedCategoryId = '';
  published = false;
  busy = false;
  error = '';
  notice = '';

  workflowForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    code: ['', [Validators.required, Validators.maxLength(100)]]
  });
  versionForm = this.fb.nonNullable.group({
    versionNumber: [1, [Validators.required, Validators.min(1)]]
  });
  stageForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    code: ['', [Validators.required, Validators.maxLength(100)]],
    sortOrder: [0, [Validators.required, Validators.min(0)]],
    requiresQualityControl: [false],
    responsibleRoleId: ['']
  });
  transitionForm = this.fb.nonNullable.group({
    fromStageId: ['', Validators.required],
    toStageId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(200)]]
  });

  constructor() {
    this.loadCategories();
    this.loadRoles();
  }

  get hasDuplicateStageCode(): boolean {
    const code = this.stageForm.controls.code.value.trim().toLocaleLowerCase();
    return !!code && this.stages.some(stage => stage.code.toLocaleLowerCase() === code);
  }

  get hasDuplicateSortOrder(): boolean {
    const order = this.stageForm.controls.sortOrder.value;
    return this.stages.some(stage => stage.sortOrder === order);
  }

  get invalidTransition(): boolean {
    const { fromStageId, toStageId } = this.transitionForm.getRawValue();
    return !fromStageId || !toStageId || fromStageId === toStageId ||
      this.transitions.some(item => item.fromStageId === fromStageId && item.toStageId === toStageId);
  }

  loadCategories(): void {
    this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({
      next: response => this.categories = response.items ?? response.data ?? response,
      error: () => this.error = 'دریافت دسته‌بندی‌ها انجام نشد.'
    });
  }

  loadRoles(): void {
    this.http.get<Role[]>(`${environment.apiUrl}/identity/roles`).subscribe({
      next: response => this.roles = response,
      error: () => this.error = 'دریافت نقش‌های مسئول مرحله انجام نشد.'
    });
  }

  roleName(id: string): string {
    return this.roles.find(role => role.id === id)?.name || 'نقش انتخاب‌شده';
  }

  createWorkflow(): void {
    if (this.workflowForm.invalid || this.busy || this.workflowId) return;
    this.clearMessages();
    this.run(this.http.post<string>(`${environment.apiUrl}/workflows`, this.workflowForm.getRawValue()), id => {
      this.workflowId = id;
      this.notice = 'فرآیند ایجاد شد. شناسه آن را برای پیگیری نگه دارید.';
    });
  }

  createVersion(): void {
    if (this.versionForm.invalid || this.busy || !this.workflowId || this.versionId) return;
    this.clearMessages();
    this.run(
      this.http.post<string>(`${environment.apiUrl}/workflows/${this.workflowId}/versions`, {
        workflowId: this.workflowId,
        ...this.versionForm.getRawValue()
      }),
      id => {
        this.versionId = id;
        this.notice = 'نسخه فرآیند ایجاد شد.';
      }
    );
  }

  addStage(): void {
    if (this.stageForm.invalid || this.busy || !this.versionId) return;
    if (this.hasDuplicateStageCode || this.hasDuplicateSortOrder) {
      this.error = this.hasDuplicateStageCode
        ? 'کد این مرحله قبلاً در همین نسخه استفاده شده است.'
        : 'ترتیب انتخاب‌شده برای مرحله دیگری ثبت شده است.';
      return;
    }

    this.clearMessages();
    const values = this.stageForm.getRawValue();
    const body = {
      workflowVersionId: this.versionId,
      ...values,
      name: values.name.trim(),
      code: values.code.trim(),
      responsibleRoleId: values.responsibleRoleId || null
    };

    this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/stages`, body), id => {
      this.stages = [...this.stages, { id, ...body }];
      this.stageForm.reset({
        name: '',
        code: '',
        sortOrder: Math.max(0, ...this.stages.map(stage => stage.sortOrder)) + 1,
        requiresQualityControl: false,
        responsibleRoleId: ''
      });
      this.notice = 'مرحله به نسخه جاری اضافه شد.';
    });
  }

  addTransition(): void {
    if (this.transitionForm.invalid || this.busy || !this.versionId) return;
    if (this.invalidTransition) {
      this.error = 'مبدأ و مقصد مسیر باید متفاوت باشند و این مسیر قبلاً ثبت نشده باشد.';
      return;
    }

    this.clearMessages();
    const values = this.transitionForm.getRawValue();
    const body = { workflowVersionId: this.versionId, ...values, name: values.name.trim() };
    this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/transitions`, body), id => {
      this.transitions = [...this.transitions, { id, ...values, name: values.name.trim() }];
      this.transitionForm.reset({ fromStageId: '', toStageId: '', name: '' });
      this.notice = 'مسیر بین مراحل ثبت شد.';
    });
  }

  publishAndAssign(): void {
    if (this.busy || this.published || !this.versionId || !this.stages.length || !this.selectedCategoryId) return;

    this.clearMessages();
    this.busy = true;
    this.http.post(`${environment.apiUrl}/workflows/versions/${this.versionId}/publish`, {}).subscribe({
      next: () => {
        this.http.post(
          `${environment.apiUrl}/categories/${this.selectedCategoryId}/workflow`,
          { categoryId: this.selectedCategoryId, workflowVersionId: this.versionId }
        ).subscribe({
          next: () => {
            this.published = true;
            this.busy = false;
            this.notice = 'نسخه منتشر و به دسته‌بندی انتخاب‌شده متصل شد.';
          },
          error: error => {
            this.error = error.error?.detail || 'نسخه منتشر شد، اما اتصال آن به دسته‌بندی انجام نشد. اتصال را جداگانه پیگیری کنید.';
            this.busy = false;
          }
        });
      },
      error: error => {
        this.error = error.error?.detail || 'انتشار فرآیند انجام نشد.';
        this.busy = false;
      }
    });
  }

  stageName(id: string): string {
    return this.stages.find(stage => stage.id === id)?.name || 'مرحله';
  }

  private clearMessages(): void {
    this.error = '';
    this.notice = '';
  }

  private run<T>(request: Observable<T>, onSuccess: (value: T) => void): void {
    this.busy = true;
    request.subscribe({
      next: value => {
        onSuccess(value);
        this.busy = false;
      },
      error: error => {
        this.error = error.error?.detail || error.error?.title || 'عملیات انجام نشد.';
        this.busy = false;
      }
    });
  }
}
