import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';

interface Transition { id:string; name:string; toStageId:string; toStageName:string; }
interface Task {
  workflowInstanceId:string; orderId:string; orderNumber:string; customerName:string;
  categoryId:string; categoryName:string; currentStageId:string; currentStageName:string;
  currentStageCode:string; requiresQualityControl:boolean; qualityControlApproved:boolean;
  canMove:boolean; canComplete:boolean; transitions:Transition[]; startedAtUtc:string;
}
interface Page {items:Task[];page:number;pageSize:number;totalCount:number;}

@Component({
 selector:'app-production',
 standalone:true,
 imports:[CommonModule,FormsModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule],
 template:`
 <section class="page" dir="rtl">
   <header class="header">
     <div>
       <span class="eyebrow">کارگاه</span>
       <h1>کارهای تولید</h1>
       <p>فقط مراحلی که می‌توانید روی آن‌ها اقدام کنید نمایش داده می‌شود.</p>
     </div>
     <button mat-icon-button type="button" aria-label="بروزرسانی" (click)="load()"><mat-icon>refresh</mat-icon></button>
   </header>

   <mat-form-field appearance="outline" class="search">
     <mat-label>جستجوی سفارش، مشتری یا مرحله</mat-label>
     <input matInput [(ngModel)]="search" (keyup.enter)="load()">
     @if(search){<button mat-icon-button matSuffix type="button" (click)="search='';load()"><mat-icon>close</mat-icon></button>}
   </mat-form-field>

   @if(loading){<div class="state"><mat-icon>sync</mat-icon>در حال دریافت کارها...</div>}
   @else if(error){<div class="error">{{error}}<button mat-button (click)="load()">تلاش مجدد</button></div>}
   @else if(!tasks.length){<div class="empty"><mat-icon>check_circle</mat-icon><strong>کاری برای اقدام ندارید</strong><span>در حال حاضر مرحله‌ای مطابق نقش شما پیدا نشد.</span></div>}
   @else {
     <div class="task-list">
       @for(task of tasks; track task.workflowInstanceId){
         <article class="task">
           <div class="task-head">
             <div>
               <strong>سفارش {{task.orderNumber}}</strong>
               <small>{{task.customerName}} · {{task.categoryName}}</small>
             </div>
             <span class="stage">{{task.currentStageName}}</span>
           </div>

           <div class="stage-box">
             <div class="stage-icon"><mat-icon>precision_manufacturing</mat-icon></div>
             <div><small>مرحله فعلی</small><strong>{{task.currentStageName}}</strong></div>
           </div>

           @if(task.requiresQualityControl){
             <div class="qc" [class.approved]="task.qualityControlApproved">
               <mat-icon>{{task.qualityControlApproved ? 'verified' : 'fact_check'}}</mat-icon>
               <div>
                 <strong>{{task.qualityControlApproved ? 'کنترل کیفیت تأیید شده' : 'نیاز به کنترل کیفیت'}}</strong>
                 <small>{{task.qualityControlApproved ? 'امکان انتقال به مرحله بعد وجود دارد.' : 'قبل از انتقال باید نتیجه QC ثبت شود.'}}</small>
               </div>
             </div>
           }

           @if(qcTaskId===task.workflowInstanceId){
             <div class="qc-form">
               <mat-form-field appearance="outline">
                 <mat-label>توضیح کنترل کیفیت (اختیاری)</mat-label>
                 <textarea matInput [(ngModel)]="qcComment" rows="2"></textarea>
               </mat-form-field>
               <div class="actions two">
                 <button mat-stroked-button type="button" class="reject" [disabled]="busy" (click)="quality(task,false)">رد</button>
                 <button mat-flat-button type="button" class="approve" [disabled]="busy" (click)="quality(task,true)">تأیید</button>
               </div>
             </div>
           } @else if(task.requiresQualityControl && !task.qualityControlApproved) {
             <button mat-stroked-button class="full" type="button" (click)="startQc(task)"><mat-icon>fact_check</mat-icon> ثبت کنترل کیفیت</button>
           }

           @if(task.transitions.length && task.canMove){
             <div class="next-title">مرحله بعد</div>
             <div class="actions">
               @for(t of task.transitions; track t.id){
                 <button mat-flat-button type="button" [disabled]="busy" (click)="move(task,t)">
                   {{t.name}} <mat-icon>arrow_back</mat-icon>
                 </button>
               }
             </div>
           }

           @if(task.canComplete){
             <button mat-flat-button class="complete" type="button" [disabled]="busy" (click)="complete(task)">
               <mat-icon>task_alt</mat-icon> پایان این فرآیند
             </button>
           }

           <button mat-button class="detail" type="button" (click)="details(task.orderId)">مشاهده جزئیات سفارش</button>
         </article>
       }
     </div>
   }
 </section>
 `,
 styles:[`
 .page{max-width:760px;margin:0 auto}.header{display:flex;align-items:center;justify-content:space-between;gap:10px;margin-bottom:14px}.eyebrow{font-size:10px;color:#286f93;font-weight:800}.header h1{font-size:23px;margin:4px 0;color:#102a35}.header p{font-size:10px;color:#7d8a8f;margin:0;line-height:1.8}.search{width:100%;margin-bottom:4px}.task-list{display:grid;gap:10px}.task{background:#fff;border:1px solid #e3eaec;border-radius:17px;padding:14px;box-shadow:0 2px 8px rgba(16,42,53,.03)}.task-head{display:flex;gap:8px;align-items:flex-start}.task-head>div{flex:1;min-width:0}.task-head strong,.task-head small{display:block}.task-head strong{font-size:14px;color:#102a35}.task-head small{font-size:10px;color:#7d8a8f;margin-top:4px;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}.stage{font-size:9px;background:#eaf3f6;color:#286f93;padding:6px 8px;border-radius:9px;white-space:nowrap}.stage-box{display:flex;align-items:center;gap:10px;background:#f7f9fa;border-radius:13px;padding:11px;margin-top:12px}.stage-icon{width:38px;height:38px;border-radius:11px;background:#e9f4f7;display:grid;place-items:center;color:#286f93}.stage-icon mat-icon{font-size:21px}.stage-box small,.stage-box strong{display:block}.stage-box small{font-size:9px;color:#89959a}.stage-box strong{font-size:13px;color:#102a35;margin-top:2px}.qc{display:flex;align-items:flex-start;gap:8px;padding:10px;margin-top:9px;border-radius:12px;background:#fff7e8;color:#8b6725}.qc.approved{background:#eaf6ef;color:#28734a}.qc mat-icon{font-size:19px;width:19px;height:19px}.qc strong,.qc small{display:block}.qc strong{font-size:10px}.qc small{font-size:9px;margin-top:3px;line-height:1.6}.qc-form{margin-top:9px}.qc-form mat-form-field{width:100%}.actions{display:flex;gap:7px;flex-wrap:wrap}.actions button{min-height:47px;border-radius:11px;flex:1}.actions.two button{flex:1}.approve{background:#28734a!important;color:#fff!important}.reject{color:#a14f4f!important;border-color:#e5bcbc!important}.full,.complete{width:100%;min-height:47px;border-radius:11px;margin-top:8px}.full{color:#286f93}.complete{background:#286f93;color:#fff}.next-title{font-size:10px;color:#7e8b90;font-weight:700;margin:12px 0 6px}.detail{width:100%;margin-top:4px;color:#286f93}.state,.empty,.error{text-align:center;padding:32px 15px;background:#fff;border-radius:16px;border:1px dashed #dce5e8;color:#7d8a8f}.state{display:flex;justify-content:center;gap:7px}.empty{display:grid;justify-items:center;gap:6px}.empty mat-icon{font-size:40px;width:40px;height:40px;color:#28734a}.empty strong{color:#102a35}.empty span{font-size:10px}.error{background:#fff0f0;color:#9d4c4c;font-size:11px}.error button{color:#286f93}@media(max-width:600px){.page{width:100%}.header h1{font-size:21px}}
 `]
})
export class ProductionComponent {
 private readonly http=inject(HttpClient); private readonly router=inject(Router);
 tasks:Task[]=[];search='';loading=false;error='';busy=false;qcTaskId='';qcComment='';totalCount=0;

 constructor(){this.load();}

 load():void{
   this.loading=true;this.error='';
   const params=new URLSearchParams({page:'1',pageSize:'30'});
   if(this.search.trim())params.set('search',this.search.trim());
   this.http.get<Page>(`${environment.apiUrl}/orders/production/tasks?${params}`).subscribe({
     next:r=>{this.tasks=r.items??[];this.totalCount=r.totalCount??0;this.loading=false;},
     error:e=>{this.error=e.error?.detail||'دریافت کارهای تولید انجام نشد.';this.loading=false;}
   });
 }

 startQc(t:Task):void{this.qcTaskId=t.workflowInstanceId;this.qcComment='';}
 quality(t:Task,approved:boolean):void{
   this.busy=true;
   this.http.post(`${environment.apiUrl}/orders/${t.orderId}/workflow/${t.categoryId}/quality-control`,{
     orderId:t.orderId,categoryId:t.categoryId,result:approved?'Approved':'Rejected',comment:this.qcComment.trim()||null
   }).subscribe({next:()=>{this.qcTaskId='';this.busy=false;this.load();},error:e=>{this.error=e.error?.detail||'ثبت کنترل کیفیت انجام نشد.';this.busy=false;}});
 }
 move(t:Task,tr:Transition):void{
   this.busy=true;
   this.http.post(`${environment.apiUrl}/orders/${t.orderId}/workflow/${t.categoryId}/transitions/${tr.id}`,{}).subscribe({
     next:()=>{this.busy=false;this.load();},error:e=>{this.error=e.error?.detail||'انتقال مرحله انجام نشد.';this.busy=false;}
   });
 }
 complete(t:Task):void{
   this.busy=true;
   this.http.post(`${environment.apiUrl}/orders/${t.orderId}/workflow/${t.categoryId}/complete`,{}).subscribe({
     next:()=>{this.busy=false;this.load();},error:e=>{this.error=e.error?.detail||'پایان فرآیند انجام نشد.';this.busy=false;}
   });
 }
 details(id:string):void{this.router.navigate(['/orders',id]);}
}