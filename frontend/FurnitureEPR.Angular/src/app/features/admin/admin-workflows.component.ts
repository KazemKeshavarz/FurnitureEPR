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
import { Observable } from 'rxjs';

interface Category { id:string; name:string; }
interface Role { id:string; name:string; permissions:string[]; }
interface Stage { id:string; name:string; code:string; sortOrder:number; requiresQualityControl:boolean; responsibleRoleId?:string|null; }
interface Transition { id:string; fromStageId:string; toStageId:string; name:string; }

@Component({
  selector:'app-admin-workflows',
  standalone:true,
  imports:[CommonModule,ReactiveFormsModule,MatCardModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule],
  templateUrl: './admin-workflows.component.html',
  styleUrl: './admin-workflows.component.scss'
})
export class AdminWorkflowsComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  workflowId=''; versionId=''; stages:Stage[]=[]; transitions:Transition[]=[]; categories:Category[]=[]; roles:Role[]=[]; selectedCategoryId=''; published=false; busy=false; error='';
  workflowForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]],code:['',[Validators.required,Validators.maxLength(50)]]});
  versionForm=this.fb.nonNullable.group({versionNumber:[1,[Validators.required,Validators.min(1)] ]});
  stageForm=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(150)]],code:['',[Validators.required,Validators.maxLength(50)]],sortOrder:[1,[Validators.required,Validators.min(1)]],requiresQualityControl:[false],responsibleRoleId:['']});
  transitionForm=this.fb.nonNullable.group({fromStageId:['',Validators.required],toStageId:['',Validators.required],name:['',[Validators.required,Validators.maxLength(150)]]});
  constructor(){this.loadCategories();this.loadRoles();}
  loadCategories():void{this.http.get<any>(`${environment.apiUrl}/categories?page=1&pageSize=100`).subscribe({next:r=>this.categories=r.items??r.data??r,error:()=>this.error='دریافت دسته‌بندی‌ها انجام نشد.'});}
  loadRoles():void{this.http.get<Role[]>(`${environment.apiUrl}/identity/roles`).subscribe({next:r=>this.roles=r,error:()=>this.error='دریافت نقش‌های مسئول مرحله انجام نشد.'});}
  roleName(id:string):string{return this.roles.find(x=>x.id===id)?.name||'نقش انتخاب‌شده';}
  createWorkflow():void{if(this.workflowForm.invalid)return;this.run(this.http.post<string>(`${environment.apiUrl}/workflows`,this.workflowForm.getRawValue()),id=>this.workflowId=id);}
  createVersion():void{if(this.versionForm.invalid)return;this.run(this.http.post<string>(`${environment.apiUrl}/workflows/${this.workflowId}/versions`,{workflowId:this.workflowId,...this.versionForm.getRawValue()}),id=>this.versionId=id);}
  addStage():void{if(this.stageForm.invalid)return;const values=this.stageForm.getRawValue();const body={workflowVersionId:this.versionId,...values,responsibleRoleId:values.responsibleRoleId||null};this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/stages`,body),id=>{this.stages=[...this.stages,{id,...values,responsibleRoleId:values.responsibleRoleId||null}];this.stageForm.reset({name:'',code:'',sortOrder:this.stages.length+1,requiresQualityControl:false,responsibleRoleId:''});});}
  addTransition():void{if(this.transitionForm.invalid)return;const body={workflowVersionId:this.versionId,...this.transitionForm.getRawValue()};this.run(this.http.post<string>(`${environment.apiUrl}/workflows/versions/${this.versionId}/transitions`,body),id=>{this.transitions=[...this.transitions,{id,...this.transitionForm.getRawValue()}];});}
  publishAndAssign():void{if(!this.selectedCategoryId)return;this.busy=true;this.http.post(`${environment.apiUrl}/workflows/versions/${this.versionId}/publish`,{}).subscribe({next:()=>this.http.post(`${environment.apiUrl}/categories/${this.selectedCategoryId}/workflow`,{categoryId:this.selectedCategoryId,workflowVersionId:this.versionId}).subscribe({next:()=>{this.published=true;this.busy=false},error:e=>{this.error=e.error?.detail||'اتصال فرآیند به دسته‌بندی انجام نشد.';this.busy=false}}),error:e=>{this.error=e.error?.detail||'انتشار فرآیند انجام نشد.';this.busy=false}});}
  stageName(id:string):string{return this.stages.find(x=>x.id===id)?.name||'مرحله';}
  private run<T>(request: Observable<T>,onSuccess:(value:T)=>void):void{this.busy=true;request.subscribe({next:(value:T)=>{onSuccess(value);this.busy=false},error:(e:any)=>{this.error=e.error?.detail||'عملیات انجام نشد.';this.busy=false}});}
}