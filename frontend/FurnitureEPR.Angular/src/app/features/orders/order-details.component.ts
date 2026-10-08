import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../environments/environment';

interface OrderItem { id:string; productId:string; productName:string; quantity:number; unitPrice:number; totalPrice:number; components:{id:string;componentId:string;componentName:string;quantity:number}[]; }
interface Workflow { id:string; categoryId:string; categoryName:string; workflowVersionId:string; workflowVersionNumber:number; currentStageId:string; currentStageName:string; status:string; startedAtUtc:string; completedAtUtc?:string|null; history:any[]; qualityChecks:any[]; }
interface Order {
  id:string; orderNumber:string; customerId:string; customerName:string; status:string;
  createdAtUtc:string; finalizedAtUtc?:string|null; createdByUserId?:string|null;
  totalAmount:number; discountAmount:number; finalAmount:number; items:OrderItem[]; workflows:Workflow[];
}

@Component({
  selector:'app-order-details',
  standalone:true,
  imports:[CommonModule,RouterLink,MatButtonModule,MatIconModule],
  template:`
    <section class="page">
      <header class="header">
        <button mat-icon-button type="button" aria-label="بازگشت" (click)="back()"><mat-icon>arrow_forward</mat-icon></button>
        <div class="title"><span>جزئیات سفارش</span><h1>{{order ? 'سفارش '+order.orderNumber : 'در حال دریافت...'}}</h1></div>
        @if(order){<span class="badge" [class]="statusClass(order.status)">{{statusText(order.status)}}</span>}
      </header>

      @if(loading){<div class="state"><mat-icon>sync</mat-icon>در حال دریافت اطلاعات...</div>}
      @else if(error){<div class="error">{{error}}<button mat-button (click)="load()">تلاش مجدد</button></div>}
      @else if(order){
        <div class="customer card">
          <div class="section-title"><mat-icon>person</mat-icon><strong>مشتری</strong></div>
          <strong class="customer-name">{{order.customerName}}</strong>
          <small>تاریخ ثبت: {{formatDateTime(order.createdAtUtc)}}</small>
        </div>

        <div class="card">
          <div class="section-title"><mat-icon>inventory_2</mat-icon><strong>اقلام سفارش</strong></div>
          <div class="items">
            @for(item of order.items; track item.id){
              <div class="item">
                <div class="item-head"><strong>{{item.productName}}</strong><span>{{item.quantity}} عدد</span></div>
                <div class="item-price">{{format(item.totalPrice)}} ریال</div>
                @if(item.components.length){
                  <div class="components">
                    @for(c of item.components; track c.id){<span>{{c.componentName}}: {{c.quantity}}</span>}
                  </div>
                }
              </div>
            }
          </div>
        </div>

        <div class="card summary">
          <div><span>مبلغ کل</span><strong>{{format(order.totalAmount)}} ریال</strong></div>
          <div><span>تخفیف</span><strong>{{format(order.discountAmount)}} ریال</strong></div>
          <div class="final"><span>مبلغ نهایی</span><strong>{{format(order.finalAmount)}} ریال</strong></div>
        </div>

        @if(order.workflows.length){
          <div class="card">
            <div class="section-title"><mat-icon>account_tree</mat-icon><strong>وضعیت تولید</strong></div>
            @for(w of order.workflows; track w.id){
              <div class="workflow">
                <div><strong>{{w.categoryName}}</strong><small>مرحله فعلی: {{w.currentStageName}}</small></div>
                <span>{{workflowStatus(w.status)}}</span>
              </div>
            }
          </div>
        }

        <a mat-flat-button class="production" routerLink="/production">
          <mat-icon>precision_manufacturing</mat-icon>
          مشاهده مراحل تولید
        </a>
      }
    </section>
  `,
  styles:[`
    .page{max-width:760px;margin:0 auto}.header{display:flex;align-items:center;gap:7px;margin-bottom:14px}.title{flex:1}.title span{font-size:10px;color:#7d8b90}.title h1{margin:3px 0 0;color:#102a35;font-size:21px}.badge{font-size:10px;font-weight:800;padding:6px 9px;border-radius:20px}.draft{background:#f2f3f4;color:#68757b}.active{background:#e9f4f8;color:#286f93}.completed{background:#eaf6ef;color:#28734a}.cancelled{background:#fff0f0;color:#a14f4f}.card{background:#fff;border:1px solid #e4eaec;border-radius:16px;padding:14px;margin-bottom:10px}.section-title{display:flex;align-items:center;gap:7px;color:#102a35;margin-bottom:12px}.section-title mat-icon{color:#286f93;font-size:19px;width:19px;height:19px}.customer-name{display:block;font-size:15px;color:#102a35}.customer small{display:block;color:#89969b;font-size:10px;margin-top:5px}.items{display:grid;gap:8px}.item{border:1px solid #edf0f1;border-radius:12px;padding:11px}.item-head{display:flex;gap:8px}.item-head strong{flex:1;color:#102a35}.item-head span{font-size:10px;color:#7e8b90}.item-price{color:#286f93;font-weight:800;font-size:12px;margin-top:7px}.components{display:flex;flex-wrap:wrap;gap:5px;margin-top:8px}.components span{font-size:9px;background:#f3f7f8;color:#718086;padding:4px 7px;border-radius:8px}.summary div{display:flex;justify-content:space-between;align-items:center;padding:6px 0;color:#7c898e;font-size:11px}.summary strong{color:#102a35}.summary .final{border-top:1px solid #edf0f1;margin-top:5px;padding-top:12px}.summary .final strong{font-size:19px;color:#286f93}.workflow{display:flex;align-items:center;gap:10px;border-top:1px solid #eef1f2;padding:11px 0}.workflow>div{flex:1}.workflow strong,.workflow small{display:block}.workflow strong{font-size:12px;color:#102a35}.workflow small{font-size:10px;color:#7f8c91;margin-top:3px}.workflow>span{font-size:9px;color:#286f93;background:#eaf3f6;padding:5px 8px;border-radius:10px}.production{width:100%;height:50px;background:#286f93;color:#fff;border-radius:12px}.state,.error{text-align:center;padding:30px;background:#fff;border-radius:15px;border:1px dashed #dce5e8;color:#7d8a8f}.state{display:flex;justify-content:center;gap:7px}.error{background:#fff0f0;color:#9d4c4c;font-size:12px}.error button{color:#286f93}@media(max-width:600px){.page{width:100%}.header{margin-bottom:10px}.badge{font-size:9px}}
  `]
})
export class OrderDetailsComponent {
  private readonly http=inject(HttpClient);
  private readonly route=inject(ActivatedRoute);
  private readonly router=inject(Router);
  order:Order|null=null; loading=false; error='';

  constructor(){this.load();}

  load():void{
    const id=this.route.snapshot.paramMap.get('id');
    if(!id){this.error='شناسه سفارش نامعتبر است.';return;}
    this.loading=true;this.error='';
    this.http.get<Order>(`${environment.apiUrl}/orders/${id}`).subscribe({
      next:o=>{this.order=o;this.loading=false;},
      error:e=>{this.error=e.error?.detail||'دریافت سفارش انجام نشد.';this.loading=false;}
    });
  }

  back():void{this.router.navigate(['/orders']);}
  statusText(s:string):string{return ({Draft:'پیش‌نویس',Active:'در حال انجام',Completed:'تکمیل‌شده',Cancelled:'لغوشده'} as Record<string,string>)[s]??s;}
  statusClass(s:string):string{return ({Draft:'draft',Active:'active',Completed:'completed',Cancelled:'cancelled'} as Record<string,string>)[s]??'draft';}
  workflowStatus(s:string):string{return s==='Completed'?'تکمیل‌شده':s==='Active'?'در حال انجام':s==='Cancelled'?'لغوشده':s;}
  format(v:number):string{return new Intl.NumberFormat('fa-IR').format(Math.round(v));}
  formatDateTime(v:string):string{return new Intl.DateTimeFormat('fa-IR',{year:'numeric',month:'2-digit',day:'2-digit',hour:'2-digit',minute:'2-digit'}).format(new Date(v));}
}
