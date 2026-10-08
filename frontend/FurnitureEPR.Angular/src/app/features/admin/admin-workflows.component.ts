import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';

interface Category { id:string; name:string; }
interface Stage { id:string; name:string; code:string; sortOrder:number; requiresQualityControl:boolean; }
interface Transition { id:string; fromStageId:string; toStageId:string; name:string; }

@Component({
  selector:'app-admin-workflows',
  standalone:true,
  imports:[CommonModule,ReactiveFormsModule,MatCardModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule],
  template:`
  <section class="page">
    <div class="heading"><span>مدیریت تولید</span><h1>فرآیندهای تولید</h1><p>فرآیند را مرحله‌به‌مرحله بسازید؛ لازم نیست از ابتدا همه چیز را بدانید.</p></div>

    <mat-card class="step" [class.done]="workflowId"><div class="step-title"><b>۱</b><div><strong>تعریف فرآیند</strong><small>نام و کد فرآیند</small></div></div>
      <form [formGroup]="workflowForm" (ngSubmit)="createWorkflow()"><mat-form-field appearance="outline"><mat-label>نام فرآیند</mat-label><input matInput formControlName="name" placeholder="مثلاً فرآیند تولید مبلمان"></mat-form-field><mat-form-field appearance="outline"><mat-label>کد</mat-label><input matInput formControlName="code" placeholder="FURNITURE"></mat-form-field><button mat-flat-button [disabled]="workflowForm.invalid || busy" type="submit">ایجاد فرآیند</button></form>
      @if(workflowId){<div class="success">فرآیند ایجاد شد.</div>}
    </mat-card>

    @if(workflowId){
      <mat-card class="step" [class.done]="versionId"><div class="step-title"><b>۲</b><div><strong>نسخه فرآیند</strong><small>هر تغییر اساسی می‌تواند نسخه‌ی جدید داشته باشد.</small></div></div>
        <form [formGroup]="versionForm" (ngSubmit)="createVersion()"><mat-form-field appearance="outline"><mat-label>شماره نسخه</mat-label><input matInput type="number" formControlName="versionNumber"></mat-form-field><button mat-flat-button [disabled]="versionForm.invalid || busy" type="submit">ساخت نسخه</button></form>
      </mat-card>
    }

    @if(versionId){
      <mat-card class="step"><div class="step-title"><b>۳</b><div><strong>مراحل فرآیند</strong><small>مراحلی مثل خیاطی، نجاری، رویه‌کوبی و کنترل کیفیت.</small></div></div>
        <form [formGroup]="stageForm" (ngSubmit)="addStage()" class="stage-form">
          <mat-form-field appearance="outline"><mat-label>نام مرحله</mat-label><input matInput formControlName="name" placeholder="مثلاً نجاری"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>کد مرحله</mat-label><input matInput formControlName="code" placeholder="CARPENTRY"></mat-form-field>
          <mat-form-field appearance="outline"><mat-label>ترتیب</mat-label><input matInput type="number" formControlName="sortOrder"></mat-form-field>
          <label class="check"><input type="checkbox" formControlName="requiresQualityControl"> نیازمند تأیید کنترل کیفیت</label>
          <button mat-flat-button [disabled]="stageForm.invalid || busy" type="submit">افزودن مرحله</button>
        </form>
        <div class="stage-list">@for(stage of stages; track stage.id){<div class="stage-row"><span>{{stage.sortOrder}}</span><div><strong>{{stage.name}}</strong><small>{{stage.code}}</small></div>@if(stage.requiresQualityControl){<em>کنترل کیفیت</em>}</div>}@empty{<div class="empty-inline">هنوز مرحله‌ای اضافه نشده است.</div>}</div>
      </mat-card>

      @if(stages.length >= 2){
        <mat-card class="step"><div class="step-title"><b>۴</b><div><strong>مسیر بین مراحل</strong><small>مشخص کنید کار از کدام مرحله به کدام مرحله برود.</small></div></div>
          <form [formGroup]="transitionForm" (ngSubmit)="addTransition()">
            <mat-form-field appearance="outline"><mat-label>از مرحله</mat-label><select matNativeControl formControlName="fromStageId">@for(stage of stages; track stage.id){<option [value]="stage.id">{{stage.sortOrder}} - {{stage.name}}</option>}</select></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>به مرحله</mat-label><select matNativeControl formControlName="toStageId">@for(stage of stages; track stage.id){<option [value]="stage.id">{{stage.sortOrder}} - {{stage.name}}</option>}</select></mat-form-field>
            <mat-form-field appearance="outline"><mat-label>عنوان مسیر</mat-label><input matInput formControlName="name" placeholder="ادامه تولید"></mat-form-field>
            <button mat-flat-button [disabled]="transitionForm.invalid || busy" type="submit">افزودن مسیر</button>
          </form>
          <div class="transition-list">@for(item of transitions; track item.id){<div class="transition"><span>{{stageName(item.fromStageId)}}</span><mat-icon>arrow_back</mat-icon><span>{{stageName(item.toStageId)}}</span><small>{{item.name}}</small></div>}@empty{<div class="empty-inline">هنوز مسیری تعریف نشده است.</div>}</div>
        </mat-card>
      }

      @if(stages.length > 0){
        <mat-card class="step"><div class="step-title"><b>۵</b><div><strong>انتشار و اتصال به دسته‌بندی</strong><small>بعد از انتشار، این نسخه قابل استفاده در سفارش‌ها می‌شود.</small></div></div>
          <div class="publish-grid"><mat-form-field appearance="outline"><mat-label>دسته‌بندی محصول</mat-label><select matNativeControl [value]="selectedCategoryId" (change)="selectedCategoryId=$any($event.target).value"><option value="">بدون اتصال</option>@for(category of categories; track category.id){<option [value]="category.id">{{category.name}}</option>}</select></mat-form-field><button mat-flat-button type="button" [disabled]="busy || published || !selectedCategoryId" (click)="publishAndAssign()"><mat-icon>{{published?'check':'publish'}}</mat-icon>{{published?'منتشر شد':'انتشار و اتصال'}}</button></div>
        </mat-card>
      }
    }

    @if(error){<div class="error">{{error}}</div>}
  </section>`,
  styles:[`
    .page{max-width:1000px;margin:0 auto}.heading span{font-size:12px;color:#286f93;font-weight:700}.heading h1{margin:5px 0 6px;color:#102a35;font-size:25px}.heading p{color:#718087;margin:0 0 18px}.step{padding:18px;border-radius:18px;margin-bottom:12px}.step-title{display:flex;gap:12px;align-items:center;margin-bottom:16px}.step-title>b{width:34px;height:34px;border-radius:50%;display:grid;place-items:center;background:#edf5f7;color:#286f93}.step-title strong,.step-title small{display:block}.step-title strong{color:#102a35}.step-title small{font-size:11px;color:#89969b;margin-top:3px}form{display:grid;grid-template-columns:1fr 1fr auto;gap:10px;align-items:start}.step button{height:56px;background:#286f93;color:#fff}.stage-form{grid-template-columns:1fr 1fr 110px}.stage-form .check{grid-column:1/-1;padding:8px 2px;color:#596970;font-size:13px}.stage-form button{grid-column:3;grid-row:1 / span 2}.stage-list{display:grid;gap:7px;margin-top:10px}.stage-row{display:flex;align-items:center;gap:10px;padding:10px;border:1px solid #e7edef;border-radius:12px}.stage-row>span{width:28px;height:28px;display:grid;place-items:center;border-radius:8px;background:#f2f6f7;color:#286f93;font-size:12px}.stage-row div{flex:1}.stage-row strong,.stage-row small{display:block}.stage-row small{font-size:10px;color:#929da1;margin-top:2px}.stage-row em{font-style:normal;font-size:10px;color:#286f93}.transition-list{display:grid;gap:7px;margin-top:10px}.transition{display:flex;align-items:center;gap:7px;padding:9px;background:#f8fafb;border-radius:10px;color:#596970}.transition span{font-size:12px}.transition small{margin-right:auto;color:#8a969a}.empty-inline{padding:15px;text-align:center;color:#929da1;font-size:12px}.publish-grid{display:grid;grid-template-columns:1fr auto;gap:10px}.publish-grid button{min-width:180px}.success{margin-top:10px;color:#2b7a55;font-size:12px}.error{margin-bottom:12px;padding:12px;border-radius:12px;background:#fff0f0;color:#a33}.done{border:1px solid #dceef2}.check input{margin-left:7px}@media(max-width:700px){.heading h1{font-size:21px}form,.stage-form,.publish-grid{grid-template-columns:1fr}.stage-form button{grid-column:auto;grid-row:auto}.step button,.publish-grid button{width:100%;height:50px}.transition{flex-wrap:wrap}.transition small{width:100%;margin-right:0}}`
  ]
})
export class AdminWorkflowsComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  workflowId=''; versionId=''; stages:Stage[]=[]; transitions:Transition[]=[]; categories:Category[]=[]; selectedCategoryId=''; published=false; busy=false; error='';
  workflowForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]],code:['',[Validators.required,Validators.maxLength(50)]]});
  versionForm=this.fb.nonNullable.group({versionNumber:[1,[Validators.required,Validators.min(1)] ]});
  stageForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]],code:['',[Validators.required,Validators.maxLength(50)]],sortOrder:[1,[Validators.required,Validators.min(1)]],requiresQualityControl:[false]});
  transitionForm=this.fb.nonNullable.group({fromStageId:['',Validators.required],toStageId:['',Validators.required],name:['',[Validators.required,Validators.maxLength(150)]]});
  constructor(){this.loadCategories();}
  loadCategories():void{this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({next:r=>this.categories=r.items??r.data??r,error:()=>this.error='دریافت دسته‌بندی‌ها انجام نشد.'});}
  createWorkflow():void{if(this.workflowForm.invalid)return;this.run(this.http.post<string>(`${environment.apiUrl}/workflows`,this.workflowForm.getRawValue()),id=>this.workflowId=id);}
  createVersion():void{if(this.versionForm.invalid)return;this.run(this.http.post<string>(`${environment.apiUrl}/workflows/${this.workflowId}/versions`,{workflowId:this.workflowId,...this.versionForm.getRawValue()}),id=>this.versionId=id);}
  addStage():void{if(this.stageForm.invalid)return;const body={workflowVersionId:this.versionId,...this.stageForm.getRawValue()};this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/stages`,body),id=>{this.stages=[...this.stages,{id,...this.stageForm.getRawValue()}];this.stageForm.patchValue({sortOrder:this.stages.length+1});});}
  addTransition():void{if(this.transitionForm.invalid)return;const body={workflowVersionId:this.versionId,...this.transitionForm.getRawValue()};this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/transitions`,body),id=>{this.transitions=[...this.transitions,{id,...this.transitionForm.getRawValue()}];});}
  publishAndAssign():void{if(!this.selectedCategoryId)return;this.busy=true;this.http.post(`${environment.apiUrl}/workflows/versions/${this.versionId}/publish`,{}).subscribe({next:()=>this.http.post(`${environment.apiUrl}/categories/${this.selectedCategoryId}/workflow`,{categoryId:this.selectedCategoryId,workflowVersionId:this.versionId}).subscribe({next:()=>{this.published=true;this.busy=false},error:e=>{this.error=e.error?.detail||'اتصال فرآیند به دسته‌بندی انجام نشد.';this.busy=false}}),error:e=>{this.error=e.error?.detail||'انتشار فرآیند انجام نشد.';this.busy=false}});}
  stageName(id:string):string{return this.stages.find(x=>x.id===id)?.name||'مرحله';}
  private run<T>(request:any,onSuccess:(value:T)=>void):void{this.busy=true;request.subscribe({next:(value:T)=>{onSuccess(value);this.busy=false},error:(e:any)=>{this.error=e.error?.detail||'عملیات انجام نشد.';this.busy=false}});}
}