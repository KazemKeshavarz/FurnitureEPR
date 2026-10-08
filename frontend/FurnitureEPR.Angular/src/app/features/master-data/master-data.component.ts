import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
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

@Component({
  selector: 'app-master-data',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTabsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatIconModule, MatCardModule],
  template: `
    <section class="page">
      <div class="heading"><span>مدیریت اطلاعات پایه</span><h1>تعاریف سامانه</h1></div>
      <mat-tab-group>
        <mat-tab label="دسته‌بندی‌ها"><div class="tab-content">
          <mat-card class="form-card"><h2>دسته‌بندی جدید</h2><form [formGroup]="categoryForm" (ngSubmit)="addCategory()">
            <mat-form-field appearance="outline"><mat-label>نام دسته‌بندی</mat-label><input matInput formControlName="name" placeholder="مثلاً مبلمان"></mat-form-field>
            <button mat-flat-button type="submit" [disabled]="categoryForm.invalid || saving">ثبت دسته‌بندی</button>
          </form></mat-card>
          <div class="list">@for (item of categories; track item.id) {<mat-card class="row"><mat-icon>category</mat-icon><div><strong>{{item.name}}</strong><small>دسته‌بندی محصول</small></div></mat-card>} @empty {<div class="empty">هنوز دسته‌بندی‌ای ثبت نشده است.</div>}</div>
        </div></mat-tab>
        <mat-tab label="محصولات"><div class="tab-content">
          <mat-card class="form-card"><h2>محصول جدید</h2><form [formGroup]="productForm" (ngSubmit)="addProduct()">
            <mat-form-field appearance="outline"><mat-label>دسته‌بندی</mat-label><select matNativeControl formControlName="categoryId"><option value="">انتخاب کنید</option>@for (item of categories; track item.id) {<option [value]="item.id">{{item.name}}</option>}</select></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>نام محصول</mat-label><input matInput formControlName="name" placeholder="مثلاً مبل بوچلی"></mat-form-field>
            <button mat-flat-button type="submit" [disabled]="productForm.invalid || saving">ثبت محصول</button>
          </form></mat-card>
          <div class="list">@for (item of products; track item.id) {<mat-card class="row"><mat-icon>chair</mat-icon><div><strong>{{item.name}}</strong><small>{{categoryName(item.categoryId)}}</small></div></mat-card>} @empty {<div class="empty">هنوز محصولی ثبت نشده است.</div>}</div>
        </div></mat-tab>
        <mat-tab label="اجزای محصول"><div class="tab-content">
          <mat-card class="form-card"><h2>جزء جدید</h2><form [formGroup]="componentForm" (ngSubmit)="addComponent()">
            <mat-form-field appearance="outline"><mat-label>نام جزء</mat-label><input matInput formControlName="name" placeholder="مثلاً پایه مبل"></mat-form-field>
            <button mat-flat-button type="submit" [disabled]="componentForm.invalid || saving">ثبت جزء</button>
          </form></mat-card>
          <div class="list">@for (item of components; track item.id) {<mat-card class="row"><mat-icon>construction</mat-icon><div><strong>{{item.name}}</strong><small>جزء قابل استفاده در محصولات</small></div></mat-card>} @empty {<div class="empty">هنوز جزئی ثبت نشده است.</div>}</div>
        </div></mat-tab>
      </mat-tab-group>
      @if(error){<div class="error">{{error}}</div>}
    </section>`,
  styles: [`
    .page{max-width:1000px;margin:0 auto}.heading span{font-size:12px;color:#286f93;font-weight:700}.heading h1{margin:5px 0 18px;color:#102a35;font-size:25px}
    .tab-content{padding:18px 2px}.form-card{padding:18px;border-radius:18px;margin-bottom:14px}.form-card h2{font-size:16px;margin:0 0 14px;color:#102a35}
    form{display:grid;grid-template-columns:1fr auto;gap:10px;align-items:start}.form-card mat-form-field{width:100%}.form-card button{height:56px;background:#286f93;color:#fff}
    .list{display:grid;gap:9px}.row{padding:14px;display:flex;align-items:center;gap:12px;border-radius:15px}.row mat-icon{color:#286f93}.row strong,.row small{display:block}.row strong{color:#102a35}.row small{color:#839096;font-size:11px;margin-top:3px}.empty{padding:28px;text-align:center;color:#8b989d;background:#fff;border-radius:16px}.error{margin-top:12px;padding:12px;border-radius:12px;background:#fff0f0;color:#a33}
    @media(max-width:700px){.heading h1{font-size:21px}.tab-content{padding-top:12px}form{grid-template-columns:1fr}.form-card button{width:100%;height:50px}.row{min-height:60px}}
  `]
})
export class MasterDataComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  categories:Category[]=[]; products:Product[]=[]; components:ComponentItem[]=[]; saving=false; error='';
  categoryForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]]});
  productForm=this.fb.nonNullable.group({categoryId:['',Validators.required],name:['',[Validators.required,Validators.maxLength(150)]]});
  componentForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]]});
  constructor(){this.loadAll();}
  loadAll():void{
    this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({next:r=>this.categories=r.items??r.data??r,error:()=>this.error='دریافت دسته‌بندی‌ها انجام نشد.'});
    this.http.get<any>(`${environment.apiUrl}/products?page=1&pageSize=100`).subscribe({next:r=>this.products=r.items??r.data??r,error:()=>this.error='دریافت محصولات انجام نشد.'});
    this.http.get<any>(`${environment.apiUrl}/components?page=1&pageSize=100`).subscribe({next:r=>this.components=r.items??r.data??r,error:()=>this.error='دریافت اجزا انجام نشد.'});
  }
  addCategory():void{if(this.categoryForm.invalid)return;this.saving=true;this.http.post(`${environment.apiUrl}/categories`,this.categoryForm.getRawValue()).subscribe({next:()=>{this.categoryForm.reset();this.loadAll();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت دسته‌بندی انجام نشد.';this.saving=false}})}
  addProduct():void{if(this.productForm.invalid)return;this.saving=true;this.http.post(`${environment.apiUrl}/products`,this.productForm.getRawValue()).subscribe({next:()=>{this.productForm.reset();this.loadAll();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت محصول انجام نشد.';this.saving=false}})}
  addComponent():void{if(this.componentForm.invalid)return;this.saving=true;this.http.post(`${environment.apiUrl}/components`,this.componentForm.getRawValue()).subscribe({next:()=>{this.componentForm.reset();this.loadAll();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت جزء انجام نشد.';this.saving=false}})}
  categoryName(id:string):string{return this.categories.find(x=>x.id===id)?.name||'بدون دسته‌بندی';}
}