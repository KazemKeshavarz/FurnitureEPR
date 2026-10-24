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
 templateUrl: './production.component.html',
  styleUrl: './production.component.scss'
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
     orderId:t.orderId,categoryId:t.categoryId,result:approved?0:1,comment:this.qcComment.trim()||null
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