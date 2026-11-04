import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth/auth.service';
import { environment } from '../../../environments/environment';

interface OrderSummary { id:string; orderNumber:string; customerName:string; status:string; }
interface Allocation { id:string; warehouseId:string; warehouseName:string; warehouseCode:string; quantity:number; }
interface OrderLine { id:string; productId:string; categoryId:string; productName:string; quantity:number; components:{componentId:string;quantity:number}[]; warehouseAllocations:Allocation[]; }
interface OrderDetail { id:string; orderNumber:string; customerName:string; status:string; items:OrderLine[]; workflows:{categoryId:string;status:string}[]; }
interface DraftLine { sourceOrderItemId:string; targetOrderItemId:string; warehouseId:string; quantity:number; productName:string; warehouseName:string; }
interface TransferRequest { id:string; sourceOrderNumber:string; sourceCustomerName:string; targetOrderNumber:string; targetCustomerName:string; reason:string; status:string; requestedAtUtc:string; lines:{id:string;sourceProductName:string;targetProductName:string;warehouseName:string;quantity:number}[]; }

@Component({
  selector:'app-order-transfers',
  standalone:true,
  imports:[CommonModule,FormsModule],
  templateUrl:'./order-transfers.component.html',
  styleUrl:'./order-transfers.component.scss'
})
export class OrderTransfersComponent {
  private readonly http=inject(HttpClient);
  private readonly auth=inject(AuthService);
  readonly canApprove=this.auth.hasPermission('workflow.manage');
  readonly canRequest=this.auth.hasPermission('workflow.move');

  sourceOrders:OrderSummary[]=[]; targetOrders:OrderSummary[]=[];
  source:OrderDetail|null=null; target:OrderDetail|null=null;
  sourceOrderId=''; targetOrderId=''; reason='';
  selectedSourceItemId=''; selectedTargetItemId=''; selectedWarehouseId=''; quantity=1;
  draftLines:DraftLine[]=[];
  requests:TransferRequest[]=[];
  loading=false; saving=false; error=''; message='';

  constructor(){
    if(this.canRequest)this.loadOrderChoices();
    if(this.canApprove)this.loadRequests();
  }

  loadOrderChoices():void{
    for (const status of ['Active','Completed','HasShortage']) {
      this.http.get<any>(`${environment.apiUrl}/orders?page=1&pageSize=100&status=${status}`).subscribe({
        next:r=>this.sourceOrders=[...this.sourceOrders,...(r.items??[])],
        error:()=>this.error='دریافت سفارش‌های آماده انتقال انجام نشد.'
      });
    }
    this.http.get<any>(`${environment.apiUrl}/orders?page=1&pageSize=100&status=Active`).subscribe({
      next:r=>this.targetOrders=r.items??[],error:()=>this.error='دریافت سفارش‌های مقصد انجام نشد.'
    });
  }

  loadSource():void{
    this.source=null;this.draftLines=[];
    if(!this.sourceOrderId)return;
    this.http.get<OrderDetail>(`${environment.apiUrl}/orders/${this.sourceOrderId}`).subscribe({
      next:o=>{this.source=o;this.selectedSourceItemId='';this.selectedWarehouseId='';},
      error:()=>this.error='دریافت جزئیات سفارش مبدأ انجام نشد.'
    });
  }

  loadTarget():void{
    this.target=null;this.draftLines=[];
    if(!this.targetOrderId)return;
    this.http.get<OrderDetail>(`${environment.apiUrl}/orders/${this.targetOrderId}`).subscribe({
      next:o=>this.target=o,error:()=>this.error='دریافت جزئیات سفارش مقصد انجام نشد.'
    });
  }

  get eligibleSourceItems():OrderLine[]{return this.source?.items.filter(item=>item.warehouseAllocations.length>0&&this.source!.workflows.some(w=>w.categoryId===item.categoryId&&w.status==='Completed'))??[];}
  get sourceItem():OrderLine|undefined{return this.eligibleSourceItems.find(x=>x.id===this.selectedSourceItemId);}
  get targetCandidates():OrderLine[]{
    if(!this.sourceItem||!this.target)return [];
    return this.target.items.filter(x=>this.sameSpecification(this.sourceItem!,x));
  }
  get selectedAllocation():Allocation|undefined{return this.sourceItem?.warehouseAllocations.find(x=>x.warehouseId===this.selectedWarehouseId);}

  sameSpecification(a:OrderLine,b:OrderLine):boolean{
    if(a.productId!==b.productId||a.components.length!==b.components.length)return false;
    const left=[...a.components].sort((x,y)=>x.componentId.localeCompare(y.componentId));
    const right=[...b.components].sort((x,y)=>x.componentId.localeCompare(y.componentId));
    return left.every((x,i)=>x.componentId===right[i].componentId&&x.quantity===right[i].quantity);
  }

  addSelectedLine():void{
    const source=this.sourceItem;
    const target=this.target?.items.find(x=>x.id===this.selectedTargetItemId);
    const allocation=this.selectedAllocation;
    if(!source||!target||!allocation||this.quantity<=0){this.error='قلم، انبار و تعداد معتبر را انتخاب کنید.';return;}
    this.addLine(source,target,allocation,this.quantity);
  }

  private addLine(source:OrderLine,target:OrderLine,allocation:Allocation,quantity:number):void{
    const already=this.draftLines.filter(x=>x.targetOrderItemId===target.id).reduce((s,x)=>s+x.quantity,0);
    const allocated=target.warehouseAllocations.reduce((s,x)=>s+x.quantity,0);
    if(quantity<=0||allocated+already+quantity>target.quantity){this.error='تعداد انتقال از نیاز باقی‌مانده سفارش مقصد بیشتر است.';return;}
    const duplicate=this.draftLines.find(x=>x.sourceOrderItemId===source.id&&x.targetOrderItemId===target.id&&x.warehouseId===allocation.warehouseId);
    if(duplicate){duplicate.quantity+=quantity;}else{
      this.draftLines.push({sourceOrderItemId:source.id,targetOrderItemId:target.id,warehouseId:allocation.warehouseId,quantity,productName:source.productName,warehouseName:allocation.warehouseName});
    }
    this.error='';this.message='';
  }

  addAllEligible():void{
    if(!this.source||!this.target)return;
    let added=0;
    for(const source of this.eligibleSourceItems){
      const target=this.target.items.find(x=>this.sameSpecification(source,x));
      if(!target)continue;
      for(const allocation of source.warehouseAllocations){
        const already=this.draftLines.filter(x=>x.targetOrderItemId===target.id).reduce((s,x)=>s+x.quantity,0);
        const allocated=target.warehouseAllocations.reduce((s,x)=>s+x.quantity,0);
        const remaining=target.quantity-allocated-already;
        if(remaining<=0)break;
        const qty=Math.min(allocation.quantity,remaining);
        if(qty>0){this.addLine(source,target,allocation,qty);added++;}
      }
    }
    if(!added)this.error='قلم آماده و هم‌مشخصات با نیاز باقی‌مانده سفارش مقصد پیدا نشد.';
  }

  removeLine(index:number):void{this.draftLines.splice(index,1);}

  submit():void{
    if(!this.source||!this.target||!this.reason.trim()||!this.draftLines.length){this.error='سفارش مبدأ، مقصد، دلیل و حداقل یک قلم انتقال الزامی است.';return;}
    this.saving=true;this.error='';this.message='';
    const payload={sourceOrderId:this.source.id,targetOrderId:this.target.id,reason:this.reason.trim(),lines:this.draftLines.map(({sourceOrderItemId,targetOrderItemId,warehouseId,quantity})=>({sourceOrderItemId,targetOrderItemId,warehouseId,quantity}))};
    this.http.post(`${environment.apiUrl}/order-transfers`,payload).subscribe({
      next:()=>{this.saving=false;this.message='درخواست انتقال برای تأیید مدیر ثبت شد.';this.sourceOrderId='';this.targetOrderId='';this.source=null;this.target=null;this.reason='';this.draftLines=[];},
      error:e=>{this.saving=false;this.error=e.error?.detail||e.error||'ثبت درخواست انتقال انجام نشد.';}
    });
  }

  loadRequests():void{
    this.loading=true;
    this.http.get<TransferRequest[]>(`${environment.apiUrl}/order-transfers`).subscribe({
      next:r=>{this.requests=r??[];this.loading=false;},error:()=>{this.error='دریافت درخواست‌های انتقال انجام نشد.';this.loading=false;}
    });
  }

  review(id:string,action:'approve'|'reject'):void{
    const title=action==='approve'?'این انتقال تأیید شود؟':'این درخواست رد شود؟';
    if(!confirm(title))return;
    this.http.post(`${environment.apiUrl}/order-transfers/${id}/${action}`,{}).subscribe({
      next:()=>{this.message=action==='approve'?'انتقال تأیید و موجودی انبار جابه‌جا شد.':'درخواست انتقال رد شد.';this.loadRequests();},
      error:e=>this.error=e.error?.detail||e.error||'بررسی درخواست انجام نشد.'
    });
  }

  statusText(s:string):string{return s==='PendingApproval'?'در انتظار تأیید':s==='Approved'?'تأییدشده':s==='Rejected'?'ردشده':s;}
  format(n:number):string{return new Intl.NumberFormat('fa-IR').format(n);}
}
