import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { environment } from '../../../environments/environment';

interface OrderItem { id:string; productId:string; productName:string; quantity:number; unitPrice:number; totalPrice:number; components:{id:string;componentId:string;componentName:string;quantity:number}[]; }
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
                <div class="workflow-head">
                  <div><strong>{{w.categoryName}}</strong><small>نسخه گردشکار {{w.workflowVersionNumber}}</small></div>
                  <span [class.done]="w.status==='Completed'">{{workflowStatus(w.status)}}</span>
                </div>
                <div class="current-stage">
                  <mat-icon>{{w.status==='Completed'?'check_circle':'play_circle'}}</mat-icon>
                  <div><small>{{w.status==='Completed'?'آخرین وضعیت':'مرحله فعلی'}}</small><strong>{{w.currentStageName}}</strong></div>
                </div>
                <div class="timeline">
                  <div class="timeline-title">مسیر طی‌شده</div>
                  <div class="timeline-row start"><span class="dot"></span><div><strong>شروع تولید</strong><small>{{formatDateTime(w.startedAtUtc)}}</small></div></div>
                  @for(h of w.history; track h.id){
                    <div class="timeline-row"><span class="dot"></span><div><strong>{{h.fromStageName}} ← {{h.toStageName}}</strong><small>{{h.transitionName}} · {{formatDateTime(h.occurredAtUtc)}}</small></div></div>
                  }
                  @if(w.status==='Completed' && w.completedAtUtc){
                    <div class="timeline-row finish"><span class="dot"></span><div><strong>پایان گردشکار</strong><small>{{formatDateTime(w.completedAtUtc)}}</small></div></div>
                  }
                  @if(!w.history.length && w.status!=='Completed'){
                    <div class="timeline-empty">هنوز مرحله‌ای ثبت نشده است.</div>
                  }
                </div>
                @if(w.qualityChecks.length){
                  <div class="qc-list">
                    <div class="timeline-title">سوابق کنترل کیفیت</div>
                    @for(q of w.qualityChecks; track q.id){
                      <div class="qc-row">
                        <div class="qc-main"><strong>{{q.stageName}}</strong><span [class.approved]="q.result==='Approved'" [class.rejected]="q.result==='Rejected'">{{qualityResult(q.result)}}</span></div>
                        @if(q.comment){<p>{{q.comment}}</p>}
                        <small>{{formatDateTime(q.checkedAtUtc)}}</small>
                      </div>
                    }
                  </div>
                }
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
    .page{max-width:760px;margin:0 auto}.header{display:flex;align-items:center;gap:7px;margin-bottom:14px}.title{flex:1}.title span{font-size:10px;color:#7d8b90}.title h1{margin:3px 0 0;color:#102a35;font-size:21px}.badge{font-size:10px;font-weight:800;padding:6px 9px;border-radius:20px}.draft{background:#f2f3f4;color:#68757b}.active{background:#e9f4f8;color:#286f93}.completed{background:#eaf6ef;color:#28734a}.cancelled{background:#fff0f0;color:#a14f4f}.card{background:#fff;border:1px solid #e4eaec;border-radius:16px;padding:14px;margin-bottom:10px}.section-title{display:flex;align-items:center;gap:7px;color:#102a35;margin-bottom:12px}.section-title mat-icon{color:#286f93;font-size:19px;width:19px;height:19px}.customer-name{display:block;font-size:15px;color:#102a35}.customer small{display:block;color:#89969b;font-size:10px;margin-top:5px}.items{display:grid;gap:8px}.item{border:1px solid #edf0f1;border-radius:12px;padding:11px}.item-head{display:flex;gap:8px}.item-head strong{flex:1;color:#102a35}.item-head span{font-size:10px;color:#7e8b90}.item-price{color:#286f93;font-weight:800;font-size:12px;margin-top:7px}.components{display:flex;flex-wrap:wrap;gap:5px;margin-top:8px}.components span{font-size:9px;background:#f3f7f8;color:#718086;padding:4px 7px;border-radius:8px}.summary div{display:flex;justify-content:space-between;align-items:center;padding:6px 0;color:#7c898e;font-size:11px}.summary strong{color:#102a35}.summary .final{border-top:1px solid #edf0f1;margin-top:5px;padding-top:12px}.summary .final strong{font-size:19px;color:#286f93}.workflow{border-top:1px solid #eef1f2;padding:13px 0}.workflow-head{display:flex;align-items:center;gap:10px}.workflow-head>div{flex:1}.workflow-head strong,.workflow-head small{display:block}.workflow-head strong{font-size:12px;color:#102a35}.workflow-head small{font-size:10px;color:#7f8c91;margin-top:3px}.workflow-head>span{font-size:9px;color:#286f93;background:#eaf3f6;padding:5px 8px;border-radius:10px}.workflow-head>span.done{color:#28734a;background:#eaf6ef}.current-stage{display:flex;align-items:center;gap:9px;margin-top:11px;padding:11px;background:#f2f7f9;border-radius:12px}.current-stage mat-icon{color:#286f93}.current-stage small,.current-stage strong{display:block}.current-stage small{font-size:9px;color:#7f8c91}.current-stage strong{font-size:13px;color:#102a35;margin-top:3px}.timeline,.qc-list{margin-top:14px}.timeline-title{font-size:10px;font-weight:800;color:#52646b;margin-bottom:9px}.timeline-row{display:flex;gap:10px;position:relative;padding:0 2px 13px;min-height:34px}.timeline-row:not(:last-child):before{content:'';position:absolute;right:7px;top:15px;bottom:0;width:1px;background:#dce7ea}.timeline-row .dot{width:12px;height:12px;flex:0 0 12px;margin-top:2px;border-radius:50%;border:2px solid #286f93;background:#fff;z-index:1}.timeline-row.finish .dot{background:#28734a;border-color:#28734a}.timeline-row strong,.timeline-row small{display:block}.timeline-row strong{font-size:11px;color:#102a35}.timeline-row small{font-size:9px;color:#89969b;margin-top:4px}.timeline-empty{font-size:10px;color:#89969b;padding:5px 0 8px}.qc-row{padding:10px 0;border-top:1px solid #edf0f1}.qc-main{display:flex;align-items:center;gap:8px}.qc-main strong{flex:1;font-size:11px;color:#102a35}.qc-main span{font-size:9px;padding:4px 7px;border-radius:8px;background:#fff0f0;color:#a14f4f}.qc-main span.approved{background:#eaf6ef;color:#28734a}.qc-main span.rejected{background:#fff0f0;color:#a14f4f}.qc-row p{margin:7px 0 0;font-size:10px;color:#65767c;white-space:pre-wrap}.qc-row small{display:block;color:#89969b;font-size:9px;margin-top:5px}.production{width:100%;height:50px;background:#286f93;color:#fff;border-radius:12px}.state,.error{text-align:center;padding:30px;background:#fff;border-radius:15px;border:1px dashed #dce5e8;color:#7d8a8f}.state{display:flex;justify-content:center;gap:7px}.error{background:#fff0f0;color:#9d4c4c;font-size:12px}.error button{color:#286f93}@media(max-width:600px){.page{width:100%}.header{margin-bottom:10px}.badge{font-size:9px}}
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
  qualityResult(s:string):string{return s==='Approved'?'تأیید شد':s==='Rejected'?'نیازمند اصلاح':s;}
  format(v:number):string{return new Intl.NumberFormat('fa-IR').format(Math.round(v));}
  formatDateTime(v:string):string{return new Intl.DateTimeFormat('fa-IR',{year:'numeric',month:'2-digit',day:'2-digit',hour:'2-digit',minute:'2-digit'}).format(new Date(v));}
}
