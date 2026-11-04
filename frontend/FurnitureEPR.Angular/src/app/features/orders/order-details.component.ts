import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../environments/environment';
import { AuthService } from '../../core/auth/auth.service';

interface Warehouse { id:string; name:string; code:string; isActive:boolean; }
interface WarehouseAllocation { id:string; warehouseId:string; warehouseName:string; warehouseCode:string; quantity:number; }
interface OrderItem { id:string; productId:string; productName:string; quantity:number; unitPrice:number; totalPrice:number; components:{id:string;componentId:string;componentName:string;quantity:number}[]; warehouseAllocations:WarehouseAllocation[]; }
interface WorkflowHistory { id:string; fromStageId:string; fromStageName:string; toStageId:string; toStageName:string; transitionId:string; transitionName:string; occurredAtUtc:string; }
interface WorkflowQualityCheck { id:string; stageId:string; stageName:string; result:string; comment?:string|null; checkedAtUtc:string; checkedByUserId?:string|null; }
interface Workflow { id:string; categoryId:string; categoryName:string; workflowVersionId:string; workflowVersionNumber:number; currentStageId:string; currentStageName:string; status:string; startedAtUtc:string; completedAtUtc?:string|null; history:WorkflowHistory[]; qualityChecks:WorkflowQualityCheck[]; }
interface Order {
  id:string; orderNumber:string; customerId:string; customerName:string; status:string;
  createdAtUtc:string; finalizedAtUtc?:string|null; createdByUserId?:string|null;
  totalAmount:number; discountAmount:number; finalAmount:number; items:OrderItem[]; workflows:Workflow[];
}

@Component({
  selector:'app-order-details',
  standalone:true,
  imports:[CommonModule,RouterLink,MatButtonModule,MatIconModule],
  templateUrl: './order-details.component.html',
  styleUrl: './order-details.component.scss'
})
export class OrderDetailsComponent {
  private readonly http=inject(HttpClient);
  private readonly route=inject(ActivatedRoute);
  private readonly router=inject(Router);
  private readonly auth=inject(AuthService);
  order:Order|null=null; loading=false; error='';
  warehouses:Warehouse[]=[]; allocationBusy=new Set<string>(); allocationError='';

  constructor(){this.load(); if(this.canManageWarehouses){this.loadWarehouses();}}

  load():void{
    const id=this.route.snapshot.paramMap.get('id');
    if(!id){this.error='شناسه سفارش نامعتبر است.';return;}
    this.loading=true;this.error='';
    this.http.get<Order>(`${environment.apiUrl}/orders/${id}`).subscribe({
      next:o=>{this.order=o;this.loading=false;},
      error:e=>{this.error=e.error?.detail||'دریافت سفارش انجام نشد.';this.loading=false;}
    });
  }

  get canManageWarehouses():boolean{return this.auth.hasPermission('workflow.manage');}

  loadWarehouses():void{
    this.http.get<Warehouse[]>(`${environment.apiUrl}/warehouses`).subscribe({
      next:items=>this.warehouses=items.filter(x=>x.isActive),
      error:()=>this.allocationError='دریافت فهرست انبارها انجام نشد.'
    });
  }

  saveAllocation(item:OrderItem, warehouseId:string, quantityValue:string):void{
    const quantity=Number(quantityValue);
    if(!warehouseId||!Number.isFinite(quantity)||quantity<=0){this.allocationError='انبار و تعداد معتبر را انتخاب کنید.';return;}
    const current=item.warehouseAllocations||[];
    const allocations=current.filter(x=>x.warehouseId!==warehouseId).map(x=>({warehouseId:x.warehouseId,quantity:x.quantity}));
    allocations.push({warehouseId,quantity});
    if(allocations.reduce((sum,x)=>sum+x.quantity,0)>item.quantity){this.allocationError='مجموع تعدادهای تخصیص‌یافته نمی‌تواند از تعداد سفارش بیشتر باشد.';return;}
    this.allocationError='';this.allocationBusy.add(item.id);
    this.http.put(`${environment.apiUrl}/orders/items/${item.id}/warehouse-allocations`,{allocations}).subscribe({
      next:()=>{const warehouse=this.warehouses.find(x=>x.id===warehouseId)!;item.warehouseAllocations=[...current.filter(x=>x.warehouseId!==warehouseId),{id:'',warehouseId,warehouseName:warehouse.name,warehouseCode:warehouse.code,quantity}];this.allocationBusy.delete(item.id);},
      error:e=>{this.allocationError=e.error?.detail||e.error||'ثبت محل نگهداری انجام نشد.';this.allocationBusy.delete(item.id);}
    });
  }

  back():void{this.router.navigate(['/orders']);}
  statusText(s:string):string{return ({Draft:'پیش‌نویس',Active:'در حال انجام',Completed:'تکمیل‌شده',HasShortage:'دارای کسری',Cancelled:'لغوشده'} as Record<string,string>)[s]??s;}
  statusClass(s:string):string{return ({Draft:'draft',Active:'active',Completed:'completed',HasShortage:'shortage',Cancelled:'cancelled'} as Record<string,string>)[s]??'draft';}
  workflowStatus(s:string):string{return s==='Completed'?'تکمیل‌شده':s==='Active'?'در حال انجام':s==='Cancelled'?'لغوشده':s;}
  qualityResult(s:string):string{return s==='Approved'?'تأیید شد':s==='Rejected'?'نیازمند اصلاح':s;}
  format(v:number):string{return new Intl.NumberFormat('fa-IR').format(Math.round(v));}
  formatDateTime(v:string):string{return new Intl.DateTimeFormat('fa-IR',{year:'numeric',month:'2-digit',day:'2-digit',hour:'2-digit',minute:'2-digit'}).format(new Date(v));}
}
