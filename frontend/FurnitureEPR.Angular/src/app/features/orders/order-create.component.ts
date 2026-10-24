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
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.scss'
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
