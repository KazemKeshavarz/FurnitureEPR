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
  template:`
    <section class="page"><div class="heading"><span>مدیریت سامانه</span><h1>نقش‌ها و دسترسی‌ها</h1><p>یک نقش بسازید و مشخص کنید هر نقش چه کارهایی می‌تواند انجام دهد.</p></div>
    <mat-card class="form-card"><h2>نقش جدید</h2><form [formGroup]="form" (ngSubmit)="createRole()"><mat-form-field appearance="outline"><mat-label>نام نقش</mat-label><input matInput formControlName="name" placeholder="مثلاً مدیر فروش"></mat-form-field><button mat-flat-button type="submit" [disabled]="form.invalid || saving"><mat-icon>add</mat-icon>ثبت نقش</button></form></mat-card>
    @if(error){<div class="error">{{error}}</div>}<div class="list">@for(role of roles; track role.id){<mat-card class="role-card"><div class="role-head"><strong>{{role.name}}</strong><small>{{role.permissions.length}} دسترسی فعال</small></div>
    <div class="permissions">@for(permission of permissions; track permission.key){<button type="button" class="permission" [class.active]="role.permissions.includes(permission.key)" (click)="toggle(role,permission.key)"><mat-icon>{{role.permissions.includes(permission.key)?'check_circle':'radio_button_unchecked'}}</mat-icon><span>{{permission.label}}</span></button>}</div>
    </mat-card>}@empty{<div class="empty">هنوز نقشی ثبت نشده است.</div>}</div></section>`,
  styles:[`.page{max-width:1000px;margin:0 auto}.heading span{font-size:12px;color:#286f93;font-weight:700}.heading h1{margin:5px 0 6px;color:#102a35;font-size:25px}.heading p{color:#718087;margin:0 0 18px}.form-card{padding:18px;border-radius:18px;margin-bottom:14px}.form-card h2{font-size:16px;color:#102a35;margin:0 0 14px}form{display:grid;grid-template-columns:1fr auto;gap:10px;align-items:start}.form-card button{height:56px;background:#286f93;color:#fff}.list{display:grid;gap:10px}.role-card{padding:16px;border-radius:17px}.role-head strong,.role-head small{display:block}.role-head strong{font-size:16px;color:#102a35}.role-head small{color:#89969b;font-size:11px;margin-top:4px}.permissions{display:grid;grid-template-columns:repeat(2,1fr);gap:8px;margin-top:14px}.permission{border:1px solid #e4ebee;background:#fff;border-radius:12px;min-height:48px;padding:8px 10px;display:flex;align-items:center;gap:8px;text-align:right;cursor:pointer;color:#596970}.permission mat-icon{font-size:21px;width:21px;height:21px;color:#9aa6ab}.permission.active{border-color:#b9d8e2;background:#f0f8fa;color:#286f93}.permission.active mat-icon{color:#286f93}.error{margin-bottom:12px;padding:12px;border-radius:12px;background:#fff0f0;color:#a33}.empty{padding:28px;text-align:center;color:#8b989d;background:#fff;border-radius:16px}@media(max-width:700px){.heading h1{font-size:21px}form{grid-template-columns:1fr}.form-card button{width:100%;height:50px}.permissions{grid-template-columns:1fr}}`]
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