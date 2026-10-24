import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { environment } from '../../../environments/environment';

interface Role { id:string; name:string; permissions:string[]; }
const PERMISSIONS=[
  {key:'workflow.manage',label:'مدیریت تعریف فرآیندها'},
  {key:'workflow.move',label:'جابجایی در فرآیند تولید'},
  {key:'workflow.quality_control',label:'تأیید کنترل کیفیت'},
  {key:'workflow.complete',label:'تکمیل مرحله تولید'},
  {key:'identity.manage_users',label:'مدیریت کاربران'},
  {key:'identity.manage_roles',label:'مدیریت نقش‌ها'},
  {key:'identity.manage_claims',label:'مدیریت دسترسی‌ها'}
];

@Component({
  selector:'app-admin-roles',standalone:true,
  imports:[CommonModule,ReactiveFormsModule,MatCardModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule],
  templateUrl: './admin-roles.component.html',
  styleUrl: './admin-roles.component.scss'
})
export class AdminRolesComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  roles:Role[]=[]; saving=false; error=''; permissions=PERMISSIONS;
  form=this.fb.nonNullable.group({name:['',[Validators.required,Validators.maxLength(100)]]});
  constructor(){this.load();}
  load():void{this.http.get<Role[]>(`${environment.apiUrl}/identity/roles`).subscribe({next:r=>this.roles=r,error:()=>this.error='دریافت نقش‌ها انجام نشد.'})}
  createRole():void{if(this.form.invalid)return;this.saving=true;this.http.post<Role>(`${environment.apiUrl}/identity/roles`,this.form.getRawValue()).subscribe({next:role=>{this.roles=[...this.roles,role];this.form.reset();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت نقش انجام نشد.';this.saving=false}})}
  toggle(role:Role,permission:string):void{
    const active=role.permissions.includes(permission);
    const request=active?this.http.delete(`${environment.apiUrl}/identity/roles/`+role.id+'/permissions',{body:{permission}}):this.http.post(`${environment.apiUrl}/identity/roles/`+role.id+'/permissions',{permission});
    request.subscribe({next:()=>this.load(),error:e=>this.error=e.error?.detail||'تغییر دسترسی انجام نشد.'});
  }
}