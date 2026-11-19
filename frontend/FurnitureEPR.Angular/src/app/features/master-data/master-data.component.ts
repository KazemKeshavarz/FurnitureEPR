import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { environment } from '../../../environments/environment';

interface Category { id: string; name: string; }
interface Product { id: string; name: string; categoryId: string; categoryName?: string; }
interface ComponentItem { id: string; name: string; }
interface ProductComponent { id:string; componentId:string; componentName:string; defaultQuantity:number; }

@Component({
  selector: 'app-master-data',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCardModule],
  templateUrl: './master-data.component.html',
  styleUrl: './master-data.component.scss'
})
export class MasterDataComponent {
  private readonly http = inject(HttpClient);
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);

  readonly initialTabIndex = Number(this.route.snapshot.data['initialTabIndex'] ?? 0);

  categories: Category[] = [];
  products: Product[] = [];
  components: ComponentItem[] = [];
  productComponents: ProductComponent[] = [];
  selectedProductName = '';
  saving = false;
  editingCategoryId: string | null = null;
  editingProductId: string | null = null;
  editingComponentId: string | null = null;
  editingProductComponentId: string | null = null;
  error = '';

  categoryForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]]
  });
  productForm = this.fb.nonNullable.group({
    categoryId: ['', Validators.required],
    name: ['', [Validators.required, Validators.maxLength(150)]]
  });
  componentForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(150)]]
  });
  productComponentForm = this.fb.nonNullable.group({
    productId: ['', Validators.required],
    componentId: ['', Validators.required],
    defaultQuantity: [1, [Validators.required, Validators.min(0.001)]]
  });

  constructor() { this.loadAll(); }

  loadAll(): void {
    this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({
      next: response => this.categories = this.unwrap<Category>(response),
      error: () => this.error = 'دریافت دسته‌بندی‌ها انجام نشد.'
    });
    this.http.get<any>(`${environment.apiUrl}/products?page=1&pageSize=100`).subscribe({
      next: response => this.products = this.unwrap<Product>(response),
      error: () => this.error = 'دریافت محصولات انجام نشد.'
    });
    this.http.get<any>(`${environment.apiUrl}/components?page=1&pageSize=100`).subscribe({
      next: response => this.components = this.unwrap<ComponentItem>(response),
      error: () => this.error = 'دریافت اجزا انجام نشد.'
    });
  }

  private unwrap<T>(response: any): T[] {
    if (Array.isArray(response)) return response as T[];
    return (response?.items ?? response?.data ?? []) as T[];
  }

  editCategory(item: Category): void {
    this.editingCategoryId = item.id;
    this.categoryForm.setValue({ name: item.name });
  }

  addCategory(): void {
    if (this.categoryForm.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    const request = this.editingCategoryId
      ? this.http.put(`${environment.apiUrl}/categories/${this.editingCategoryId}`, { id: this.editingCategoryId, ...this.categoryForm.getRawValue() })
      : this.http.post(`${environment.apiUrl}/categories`, this.categoryForm.getRawValue());
    request.subscribe({
      next: () => {
        this.categoryForm.reset({ name: '' });
        this.editingCategoryId = null;
        this.loadAll();
        this.saving = false;
      },
      error: response => {
        this.error = response.error?.detail || 'ثبت دسته‌بندی انجام نشد.';
        this.saving = false;
      }
    });
  }

  editProduct(item: Product): void {
    this.editingProductId = item.id;
    this.productForm.setValue({ categoryId: item.categoryId, name: item.name });
  }

  addProduct(): void {
    if (this.productForm.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    const request = this.editingProductId
      ? this.http.put(`${environment.apiUrl}/products/${this.editingProductId}`, { id: this.editingProductId, ...this.productForm.getRawValue() })
      : this.http.post(`${environment.apiUrl}/products`, this.productForm.getRawValue());
    request.subscribe({
      next: () => {
        this.productForm.reset({ categoryId: '', name: '' });
        this.editingProductId = null;
        this.loadAll();
        this.saving = false;
      },
      error: response => {
        this.error = response.error?.detail || 'ثبت محصول انجام نشد.';
        this.saving = false;
      }
    });
  }

  editComponent(item: ComponentItem): void {
    this.editingComponentId = item.id;
    this.componentForm.setValue({ name: item.name });
  }

  addComponent(): void {
    if (this.componentForm.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    const request = this.editingComponentId
      ? this.http.put(`${environment.apiUrl}/components/${this.editingComponentId}`, this.componentForm.getRawValue())
      : this.http.post(`${environment.apiUrl}/components`, this.componentForm.getRawValue());
    request.subscribe({
      next: () => {
        this.componentForm.reset({ name: '' });
        this.editingComponentId = null;
        this.loadAll();
        this.saving = false;
      },
      error: response => {
        this.error = response.error?.detail || 'ثبت جزء انجام نشد.';
        this.saving = false;
      }
    });
  }

  categoryName(id: string): string {
    return this.categories.find(item => item.id === id)?.name || 'بدون دسته‌بندی';
  }

  loadProductComponents(id: string): void {
    this.productComponents = [];
    const product = this.products.find(item => item.id === id);
    this.selectedProductName = product?.name || '';
    if (!id) return;
    this.http.get<any>(`${environment.apiUrl}/products/${id}`).subscribe({
      next: response => this.productComponents = response.components ?? [],
      error: () => this.error = 'دریافت اجزای محصول انجام نشد.'
    });
  }

  editProductComponent(item: ProductComponent): void {
    this.editingProductComponentId = item.id;
    this.productComponentForm.patchValue({ productId: this.productComponentForm.controls.productId.value, componentId: item.componentId, defaultQuantity: item.defaultQuantity });
  }

  addProductComponent(): void {
    if (this.productComponentForm.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    const value = this.productComponentForm.getRawValue();
    const request = this.editingProductComponentId
      ? this.http.put(`${environment.apiUrl}/products/${value.productId}/components/${this.editingProductComponentId}`, { defaultQuantity: value.defaultQuantity })
      : this.http.post(`${environment.apiUrl}/products/${value.productId}/components`, { componentId: value.componentId, defaultQuantity: value.defaultQuantity });
    request.subscribe({
      next: () => {
        this.loadProductComponents(value.productId);
        this.productComponentForm.patchValue({ componentId: '', defaultQuantity: 1 });
        this.editingProductComponentId = null;
        this.saving = false;
      },
      error: response => {
        this.error = response.error?.detail || 'افزودن جزء به محصول انجام نشد.';
        this.saving = false;
      }
    });
  }
}
