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
interface Role { id: string; name: string; }
interface WorkflowListItem {
  id: string; name: string; code: string; versionCount: number;
  publishedVersionCount: number; assignedCategoryNames: string[];
}
interface Stage {
  id: string; name: string; code: string; sortOrder: number;
  requiresQualityControl: boolean; responsibleRoleId?: string | null; isActive: boolean;
}
interface Transition { id: string; fromStageId: string; toStageId: string; name: string; }
interface WorkflowVersion {
  id: string; versionNumber: number; isPublished: boolean;
  stages: Stage[]; transitions: Transition[];
}
interface WorkflowDetail {
  id: string; name: string; code: string;
  assignedCategoryIds: string[]; versions: WorkflowVersion[];
}

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

  workflows: WorkflowListItem[] = [];
  categories: Category[] = [];
  roles: Role[] = [];
  selectedWorkflow: WorkflowDetail | null = null;
  selectedVersion: WorkflowVersion | null = null;
  workflowId = '';
  versionId = '';
  stages: Stage[] = [];
  transitions: Transition[] = [];
  selectedCategoryId = '';
  published = false;
  busy = false;
  loadingWorkflows = false;
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
    this.loadWorkflows();
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

  get canEditVersion(): boolean {
    return !!this.versionId && !this.published;
  }

  loadWorkflows(): void {
    this.loadingWorkflows = true;
    this.http.get<any>(`${environment.apiUrl}/workflows?page=1&pageSize=100`).subscribe({
      next: response => {
        this.workflows = response.items ?? [];
        this.loadingWorkflows = false;
      },
      error: error => {
        this.error = error.error?.detail || 'دریافت فرآیندهای ثبت‌شده انجام نشد.';
        this.loadingWorkflows = false;
      }
    });
  }

  selectWorkflow(id: string): void {
    if (!id) {
      this.resetSelection();
      return;
    }

    this.clearMessages();
    this.busy = true;
    this.http.get<WorkflowDetail>(`${environment.apiUrl}/workflows/${id}`).subscribe({
      next: detail => {
        this.selectedWorkflow = detail;
        this.workflowId = detail.id;
        this.selectedVersion = null;
        this.versionId = '';
        this.stages = [];
        this.transitions = [];
        this.published = false;
        this.selectedCategoryId = detail.assignedCategoryIds[0] || '';
        const latest = [...detail.versions].sort((a, b) => b.versionNumber - a.versionNumber)[0];
        this.versionForm.reset({ versionNumber: latest ? latest.versionNumber + 1 : 1 });
        this.busy = false;
        this.notice = 'اطلاعات فرآیند از سرور دریافت شد.';
      },
      error: error => {
        this.error = error.error?.detail || 'دریافت جزئیات فرآیند انجام نشد.';
        this.busy = false;
      }
    });
  }

  selectVersion(id: string): void {
    const version = this.selectedWorkflow?.versions.find(item => item.id === id) ?? null;
    this.selectedVersion = version;
    this.versionId = version?.id || '';
    this.stages = version ? [...version.stages] : [];
    this.transitions = version ? [...version.transitions] : [];
    this.published = version?.isPublished ?? false;
    this.clearMessages();
    if (version) this.notice = this.published
      ? 'این نسخه منتشر شده و فقط قابل مشاهده است. برای تغییرات، نسخه جدید بسازید.'
      : 'نسخه انتخاب‌شده آماده ویرایش است.';
  }

  private resetSelection(): void {
    this.selectedWorkflow = null;
    this.selectedVersion = null;
    this.workflowId = '';
    this.versionId = '';
    this.stages = [];
    this.transitions = [];
    this.published = false;
    this.selectedCategoryId = '';
    this.versionForm.reset({ versionNumber: 1 });
    this.clearMessages();
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
    if (this.workflowForm.invalid || this.busy) return;
    this.clearMessages();
    this.busy = true;
    this.http.post<string>(`${environment.apiUrl}/workflows`, this.workflowForm.getRawValue()).subscribe({
      next: id => {
        this.workflowId = id;
        this.loadWorkflows();
        this.http.get<WorkflowDetail>(`${environment.apiUrl}/workflows/${id}`).subscribe({
          next: detail => {
            this.selectedWorkflow = detail;
            this.busy = false;
            this.notice = 'فرآیند ایجاد شد و اکنون در فهرست فرآیندها قابل مشاهده است.';
          },
          error: error => {
            this.busy = false;
            this.error = error.error?.detail || 'فرآیند ساخته شد، اما دریافت جزئیات آن انجام نشد.';
          }
        });
      },
      error: error => {
        this.busy = false;
        this.error = error.error?.detail || 'ایجاد فرآیند انجام نشد.';
      }
    });
  }

  createVersion(): void {
    if (this.versionForm.invalid || this.busy || !this.workflowId) return;
    this.clearMessages();
    this.busy = true;
    this.http.post<string>(`${environment.apiUrl}/workflows/${this.workflowId}/versions`, {
      workflowId: this.workflowId,
      ...this.versionForm.getRawValue()
    }).subscribe({
      next: id => {
        this.http.get<WorkflowDetail>(`${environment.apiUrl}/workflows/${this.workflowId}`).subscribe({
          next: detail => {
            this.selectedWorkflow = detail;
            this.selectVersion(id);
            this.busy = false;
            this.notice = 'نسخه جدید ساخته شد و آماده تعریف مراحل است.';
          },
          error: error => {
            this.busy = false;
            this.error = error.error?.detail || 'نسخه ساخته شد، اما دریافت جزئیات آن انجام نشد.';
          }
        });
      },
      error: error => {
        this.busy = false;
        this.error = error.error?.detail || 'ساخت نسخه جدید انجام نشد.';
      }
    });
  }

  addStage(): void {
    if (this.stageForm.invalid || this.busy || !this.versionId || this.published) return;
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
      this.stages = [...this.stages, { id, ...body, isActive: true }];
      this.syncSelectedVersion();
      this.stageForm.reset({
        name: '', code: '',
        sortOrder: Math.max(0, ...this.stages.map(stage => stage.sortOrder)) + 1,
        requiresQualityControl: false, responsibleRoleId: ''
      });
      this.notice = 'مرحله به نسخه جاری اضافه شد.';
    });
  }

  addTransition(): void {
    if (this.transitionForm.invalid || this.busy || !this.versionId || this.published) return;
    if (this.invalidTransition) {
      this.error = 'مبدأ و مقصد مسیر باید متفاوت باشند و این مسیر قبلاً ثبت نشده باشد.';
      return;
    }

    this.clearMessages();
    const values = this.transitionForm.getRawValue();
    const body = { workflowVersionId: this.versionId, ...values, name: values.name.trim() };
    this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/transitions`, body), id => {
      this.transitions = [...this.transitions, { id, ...values, name: values.name.trim() }];
      this.syncSelectedVersion();
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
        this.published = true;
        this.syncSelectedVersion();
        this.http.post(
          `${environment.apiUrl}/categories/${this.selectedCategoryId}/workflow`,
          { categoryId: this.selectedCategoryId, workflowVersionId: this.versionId }
        ).subscribe({
          next: () => {
            if (this.selectedWorkflow && !this.selectedWorkflow.assignedCategoryIds.includes(this.selectedCategoryId)) {
              this.selectedWorkflow = {
                ...this.selectedWorkflow,
                assignedCategoryIds: [...this.selectedWorkflow.assignedCategoryIds, this.selectedCategoryId]
              };
            }
            this.busy = false;
            this.notice = 'نسخه منتشر و به دسته‌بندی انتخاب‌شده متصل شد.';
            this.loadWorkflows();
          },
          error: error => {
            this.error = error.error?.detail || 'نسخه منتشر شد، اما اتصال به دسته‌بندی انجام نشد. برای اتصال مجدد، نسخه دیگری منتشر نکنید.';
            this.busy = false;
            this.loadWorkflows();
          }
        });
      },
      error: error => {
        this.error = error.error?.detail || 'انتشار فرآیند انجام نشد.';
        this.busy = false;
      }
    });
  }

  assignSelectedVersion(): void {
    if (this.busy || !this.published || !this.versionId || !this.selectedCategoryId) return;
    this.clearMessages();
    this.busy = true;
    this.http.post(
      `${environment.apiUrl}/categories/${this.selectedCategoryId}/workflow`,
      { categoryId: this.selectedCategoryId, workflowVersionId: this.versionId }
    ).subscribe({
      next: () => {
        if (this.selectedWorkflow && !this.selectedWorkflow.assignedCategoryIds.includes(this.selectedCategoryId)) {
          this.selectedWorkflow = {
            ...this.selectedWorkflow,
            assignedCategoryIds: [...this.selectedWorkflow.assignedCategoryIds, this.selectedCategoryId]
          };
        }
        this.busy = false;
        this.notice = 'نسخه منتشرشده به دسته‌بندی متصل شد.';
        this.loadWorkflows();
      },
      error: error => {
        this.busy = false;
        this.error = error.error?.detail || 'اتصال نسخه به دسته‌بندی انجام نشد.';
      }
    });
  }

  stageName(id: string): string {
    return this.stages.find(stage => stage.id === id)?.name || 'مرحله';
  }

  private syncSelectedVersion(): void {
    if (!this.selectedWorkflow || !this.selectedVersion) return;
    const updated = { ...this.selectedVersion, stages: [...this.stages], transitions: [...this.transitions] };
    this.selectedVersion = updated;
    this.selectedWorkflow = {
      ...this.selectedWorkflow,
      versions: this.selectedWorkflow.versions.map(item => item.id === updated.id ? updated : item)
    };
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
