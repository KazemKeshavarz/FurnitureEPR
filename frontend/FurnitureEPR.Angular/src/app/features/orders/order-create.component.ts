import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';

interface Customer { id:string; name:string; phoneNumber?:string; }
interface Category { id:string; name:string; }
interface Product { id:string; name:string; categoryId:string; categoryName?:string; }
interface ProductComponent { id:string; componentId:string; componentName:string; defaultQuantity:number; }
interface ProductDetails extends Product { components:ProductComponent[]; }
interface OrderComponent { componentId:string; componentName:string; quantity:number; }
interface CartItem { productId:string; productName:string; quantity:number; unitPrice:number; components:OrderComponent[]; }

@Component({
  selector:'app-order-create',
  standalone:true,
  imports:[CommonModule,ReactiveFormsModule,MatCardModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule],
  template:`
  <section class="page">
    <div class="heading">
      <span>عملیات فروش</span>
      <h1>ثبت سفارش جدید</h1>
      <p>مراحل را ساده و به‌ترتیب انجام دهید.</p>
    </div>

    <mat-card class="step">
      <div class="step-title"><b>۱</b><div><strong>مشتری</strong><small>مشتری سفارش را انتخاب کنید.</small></div></div>
      <div class="customer-actions">
        <mat-form-field appearance="outline">
          <mat-label>انتخاب مشتری</mat-label>
          <select matNativeControl [value]="customerId" (change)="customerId=$any($event.target).value">
            <option value="">انتخاب کنید</option>
            @for(c of customers; track c.id){<option [value]="c.id">{{c.name}}{{c.phoneNumber ? ' - '+c.phoneNumber : ''}}</option>}
          </select>
        </mat-form-field>
        <button mat-stroked-button type="button" (click)="showNewCustomer=!showNewCustomer">
          <mat-icon>person_add</mat-icon> مشتری جدید
        </button>
      </div>

      @if(showNewCustomer){
        <form [formGroup]="customerForm" (ngSubmit)="createCustomer()" class="new-customer">
          <mat-form-field appearance="outline"><mat-label>نام مشتری</mat-label><input matInput formControlName="name"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>شماره تماس</mat-label><input matInput formControlName="phoneNumber" inputmode="tel"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>آدرس</mat-label><input matInput formControlName="address"></mat-form-field>
          <button mat-flat-button type="submit" [disabled]="customerForm.invalid || busy">ثبت و انتخاب مشتری</button>
        </form>
      }
    </mat-card>

    @if(customerId){
      <mat-card class="step">
        <div class="step-title"><b>۲</b><div><strong>افزودن محصول</strong><small>دسته‌بندی، محصول و تعداد را انتخاب کنید.</small></div></div>

        <div class="product-grid">
          <mat-form-field appearance="outline">
            <mat-label>دسته‌بندی</mat-label>
            <select matNativeControl [value]="categoryId" (change)="selectCategory($any($event.target).value)">
              <option value="">انتخاب کنید</option>
              @for(c of categories; track c.id){<option [value]="c.id">{{c.name}}</option>}
            </select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>محصول</mat-label>
            <select matNativeControl [value]="productId" (change)="selectProduct($any($event.target).value)" [disabled]="!categoryId">
              <option value="">انتخاب کنید</option>
              @for(p of products; track p.id){<option [value]="p.id">{{p.name}}</option>}
            </select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>تعداد</mat-label>
            <input matInput type="number" min="0.001" step="1" [value]="itemQuantity" (input)="itemQuantity=+$any($event.target).value" inputmode="decimal">
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>قیمت واحد (ریال)</mat-label>
            <input matInput type="number" min="0" step="1000" [value]="itemUnitPrice" (input)="itemUnitPrice=+$any($event.target).value" inputmode="numeric">
          </mat-form-field>
        </div>

        @if(selectedProduct){
          <div class="component-box">
            <div class="box-title"><mat-icon>construction</mat-icon><strong>اجزای محصول</strong><small>مقادیر پیش‌فرض را در صورت نیاز تغییر دهید.</small></div>
            @for(c of selectedComponents; track c.componentId){
              <div class="component-row">
                <div><strong>{{c.componentName}}</strong><small>مقدار پیش‌فرض: {{c.defaultQuantity}}</small></div>
                <input type="number" min="0" step="0.001" [value]="c.quantity" (input)="c.quantity=+$any($event.target).value">
              </div>
            }
            @empty {<div class="empty-inline">برای این محصول جزء پیش‌فرض تعریف نشده است.</div>}
          </div>

          <button mat-flat-button class="add-product" type="button" [disabled]="!canAddItem()" (click)="addItem()">
            <mat-icon>add_shopping_cart</mat-icon> افزودن به سفارش
          </button>
        }
      </mat-card>
    }

    @if(items.length){
      <mat-card class="step">
        <div class="step-title"><b>۳</b><div><strong>اقلام سفارش</strong><small>قیمت و تعداد هر قلم را بررسی کنید.</small></div></div>
        <div class="cart">
          @for(item of items; track $index){
            <div class="cart-item">
              <div class="cart-head">
                <div><strong>{{item.productName}}</strong><small>{{item.quantity}} عدد × {{format(item.unitPrice)}} ریال</small></div>
                <button mat-icon-button type="button" aria-label="حذف" (click)="removeItem($index)"><mat-icon>delete_outline</mat-icon></button>
              </div>
              <div class="cart-total">{{format(item.quantity * item.unitPrice)}} ریال</div>
            </div>
          }
        </div>
      </mat-card>

      <mat-card class="summary">
        <div class="summary-row"><span>مبلغ کل سفارش</span><strong>{{format(totalAmount)}} ریال</strong></div>
        <div class="discount-row">
          <mat-form-field appearance="outline">
            <mat-label>تخفیف (ریال)</mat-label>
            <input matInput type="number" min="0" [value]="discountAmount" (input)="discountAmount=+$any($event.target).value" inputmode="numeric">
          </mat-form-field>
        </div>
        <div class="final-row"><span>مبلغ نهایی</span><strong>{{format(finalAmount)}} ریال</strong></div>
        @if(discountAmount > totalAmount){<div class="error-inline">مبلغ تخفیف نمی‌تواند بیشتر از مبلغ کل باشد.</div>}
        <button mat-flat-button class="save" type="button" [disabled]="busy || !canSave()" (click)="saveOrder()">
          <mat-icon>check_circle</mat-icon> ثبت و نهایی‌سازی سفارش
        </button>
      </mat-card>
    }

    @if(success){
      <mat-card class="success-card">
        <mat-icon>task_alt</mat-icon>
        <strong>سفارش با موفقیت ثبت شد.</strong>
        <span>شماره سفارش: {{successOrderNumber}}</span>
        <button mat-stroked-button type="button" (click)="newOrder()">ثبت سفارش جدید</button>
      </mat-card>
    }

    @if(error){<div class="error">{{error}}</div>}
  </section>`,
  styles:[`
    .page{max-width:1000px;margin:0 auto}.heading span{font-size:12px;color:#286f93;font-weight:700}.heading h1{margin:5px 0 6px;color:#102a35;font-size:25px}.heading p{color:#718087;margin:0 0 18px}
    .step,.summary,.success-card{padding:18px;border-radius:18px;margin-bottom:12px}.step-title{display:flex;gap:12px;align-items:center;margin-bottom:16px}.step-title>b{width:34px;height:34px;border-radius:50%;display:grid;place-items:center;background:#edf5f7;color:#286f93}.step-title strong,.step-title small{display:block}.step-title strong{color:#102a35}.step-title small{font-size:11px;color:#89969b;margin-top:3px}
    .customer-actions,.product-grid,.new-customer{display:grid;grid-template-columns:1fr auto;gap:10px;align-items:start}.customer-actions button{height:56px}.new-customer{grid-template-columns:1fr 1fr 1fr auto;margin-top:4px}.new-customer button{height:56px}
    .product-grid{grid-template-columns:1fr 1fr 120px 180px}.component-box{margin-top:4px;border:1px solid #e4ebed;border-radius:15px;padding:12px}.box-title{display:flex;align-items:center;gap:7px;margin-bottom:9px}.box-title mat-icon{color:#286f93}.box-title small{color:#8b989d;font-size:10px;margin-right:auto}.component-row{display:flex;align-items:center;gap:12px;padding:9px 2px;border-top:1px solid #eef1f2}.component-row div{flex:1}.component-row strong,.component-row small{display:block}.component-row strong{font-size:12px;color:#102a35}.component-row small{font-size:10px;color:#929da1;margin-top:2px}.component-row input{width:90px;height:40px;border:1px solid #d7e0e3;border-radius:9px;padding:0 8px;text-align:center}
    .add-product,.save{width:100%;height:52px;background:#286f93;color:#fff;margin-top:12px}.cart{display:grid;gap:8px}.cart-item{padding:12px;border:1px solid #e7edef;border-radius:13px}.cart-head{display:flex;align-items:center}.cart-head>div{flex:1}.cart-head strong,.cart-head small{display:block}.cart-head strong{color:#102a35}.cart-head small{font-size:11px;color:#849197;margin-top:3px}.cart-head button{color:#a65b5b}.cart-total{text-align:left;color:#286f93;font-weight:800;font-size:13px}
    .summary-row,.final-row{display:flex;justify-content:space-between;align-items:center}.summary-row span,.final-row span{color:#6d7b81}.summary-row strong{color:#102a35}.discount-row{margin-top:12px}.discount-row mat-form-field{width:100%}.final-row{border-top:1px solid #e5eaec;padding-top:12px}.final-row strong{font-size:20px;color:#286f93}.error-inline{color:#a33;font-size:11px;margin:6px 0}.success-card{display:grid;justify-items:center;gap:7px;text-align:center}.success-card mat-icon{font-size:42px;width:42px;height:42px;color:#286f93}.success-card strong{color:#102a35}.success-card span{color:#68777d}.error{padding:12px;border-radius:12px;background:#fff0f0;color:#a33;margin-top:10px}.empty-inline{padding:14px;text-align:center;color:#929da1;font-size:12px}
    @media(max-width:700px){.heading h1{font-size:21px}.customer-actions,.product-grid,.new-customer{grid-template-columns:1fr}.customer-actions button,.new-customer button{width:100%;height:50px}.component-row input{width:80px}.step,.summary{padding:14px}}
  `]
})
export class OrderCreateComponent {
  private readonly http=inject(HttpClient);
  private readonly fb=inject(FormBuilder);
  private readonly router=inject(Router);

  customers:Customer[]=[]; categories:Category[]=[]; products:Product[]=[]; selectedComponents:(ProductComponent & {quantity:number})[]=[];
  items:CartItem[]=[]; customerId=''; categoryId=''; productId=''; selectedProduct:ProductDetails|null=null;
  itemQuantity=1; itemUnitPrice=0; discountAmount=0; busy=false; error=''; success=false; successOrderNumber='';
  showNewCustomer=false;
  customerForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(200)]],phoneNumber:[''],address:['']});

  constructor(){this.loadCustomers();this.loadCategories();}

  loadCustomers():void{this.http.get<any>(`${environment.apiUrl}/customers?page=1&pageSize=100`).subscribe({next:r=>this.customers=r.items??r.data??r,error:()=>this.error='دریافت مشتریان انجام نشد.'});}
  loadCategories():void{this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({next:r=>this.categories=r.items??r.data??r,error:()=>this.error='دریافت دسته‌بندی‌ها انجام نشد.'});}
  selectCategory(id:string):void{this.categoryId=id;this.productId='';this.selectedProduct=null;this.selectedComponents=[];if(!id)return;this.http.get<any>(`${environment.apiUrl}/products?page=1&pageSize=100&categoryId=${id}`).subscribe({next:r=>this.products=r.items??r.data??r,error:()=>this.error='دریافت محصولات انجام نشد.'});}
  selectProduct(id:string):void{this.productId=id;this.selectedProduct=null;this.selectedComponents=[];if(!id)return;this.http.get<ProductDetails>(`${environment.apiUrl}/products/${id}`).subscribe({next:p=>{this.selectedProduct=p;this.selectedComponents=(p.components??[]).map(c=>({...c,quantity:c.defaultQuantity}));},error:()=>this.error='دریافت اطلاعات محصول انجام نشد.'});}
  createCustomer():void{if(this.customerForm.invalid)return;this.busy=true;this.http.post<string>(`${environment.apiUrl}/customers`,this.customerForm.getRawValue()).subscribe({next:id=>{const value=this.customerForm.getRawValue();this.customers=[...this.customers,{id,name:value.name,phoneNumber:value.phoneNumber}];this.customerId=id;this.customerForm.reset();this.showNewCustomer=false;this.busy=false;},error:e=>{this.error=e.error?.detail||'ثبت مشتری انجام نشد.';this.busy=false;}});}
  canAddItem():boolean{return !!this.selectedProduct&&this.itemQuantity>0&&this.itemUnitPrice>=0&&this.selectedComponents.every(x=>x.quantity>=0);}
  addItem():void{if(!this.canAddItem()||!this.selectedProduct)return;this.items=[...this.items,{productId:this.selectedProduct.id,productName:this.selectedProduct.name,quantity:this.itemQuantity,unitPrice:this.itemUnitPrice,components:this.selectedComponents.map(x=>({componentId:x.componentId,componentName:x.componentName,quantity:x.quantity}))}];this.productId='';this.selectedProduct=null;this.selectedComponents=[];this.itemQuantity=1;this.itemUnitPrice=0;}
  removeItem(index:number):void{this.items=this.items.filter((_,i)=>i!==index);}
  get totalAmount():number{return this.items.reduce((sum,x)=>sum+(x.quantity*x.unitPrice),0);}
  get finalAmount():number{return Math.max(0,this.totalAmount-this.discountAmount);}
  canSave():boolean{return !!this.customerId&&this.items.length>0&&this.discountAmount>=0&&this.discountAmount<=this.totalAmount;}
  saveOrder():void{if(!this.canSave())return;this.busy=true;this.error='';const body={customerId:this.customerId,createdByUserId:null,discountAmount:this.discountAmount,items:this.items.map(x=>({productId:x.productId,quantity:x.quantity,unitPrice:x.unitPrice,components:x.components.map(c=>({componentId:c.componentId,quantity:c.quantity}))}))};this.http.post<string>(`${environment.apiUrl}/orders`,body).subscribe({next:id=>this.finalize(id),error:e=>{this.error=e.error?.detail||'ثبت سفارش انجام نشد.';this.busy=false;}});}
  finalize(id:string):void{this.http.post(`${environment.apiUrl}/orders/${id}/finalize`,{}).subscribe({next:()=>this.http.get<any>(`${environment.apiUrl}/orders/${id}`).subscribe({next:o=>{this.success=true;this.successOrderNumber=o.orderNumber;this.busy=false;},error:()=>{this.success=true;this.successOrderNumber='ثبت شد';this.busy=false;}}),error:e=>{this.error=e.error?.detail||'نهایی‌سازی سفارش انجام نشد.';this.busy=false;}});}
  format(value:number):string{return new Intl.NumberFormat('fa-IR').format(Math.round(value));}
  newOrder():void{this.items=[];this.customerId='';this.categoryId='';this.productId='';this.selectedProduct=null;this.selectedComponents=[];this.discountAmount=0;this.success=false;this.successOrderNumber='';}
}
