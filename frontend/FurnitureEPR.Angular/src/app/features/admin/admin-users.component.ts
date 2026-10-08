import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { environment } from '../../../environments/environment';

interface Role { id:string; name:string; permissions:string[]; }
interface User { id:string; userName:string; email?:string; emailConfirmed:boolean; lockedOut:boolean; roles:Role[]; directPermissions:string[]; }

@Component({
  selector:'app-admin-users',
  standalone:true,
  imports:[CommonModule,ReactiveFormsModule,MatCardModule,MatButtonModule,MatIconModule,MatFormFieldModule,MatInputModule,MatSelectModule],
  template: `
    <section class="page">
      <div class="heading"><span>مدیریت سامانه</span><h1>کاربران</h1><p>کاربران سامانه را ایجاد کنید و نقش مناسب را به هر کاربر بدهید.</p></div>
      <mat-card class="form-card"><h2>افزودن کاربر</h2><form [formGroup]="form" (ngSubmit)="createUser()">
        <mat-form-field appearance="outline"><mat-label>نام کاربری</mat-label><input matInput formControlName="userName"></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>ایمیل</mat-label><input matInput type="email" formControlName="email"></mat-form-field>
        <mat-form-field appearance="outline"><mat-label>رمز عبور</mat-label><input matInput type="password" formControlName="password"></mat-form-field>
        <button mat-flat-button type="submit" [disabled]="form.invalid || saving"><mat-icon>person_add</mat-icon>ثبت کاربر</button>
      </form></mat-card>
      @if(error){<div class="error">{{error}}</div>}
      <div class="list">@for(user of users; track user.id){<mat-card class="user-card">
        <div class="user-main"><div class="avatar"><mat-icon>person</mat-icon></div><div class="identity"><strong>{{user.userName}}</strong><small>{{user.email || 'ایمیل ثبت نشده'}}</small></div>@if(user.lockedOut){<span class="locked">قفل شده</span>}</div>
        <div class="roles">@for(role of user.roles; track role.id){<span>{{role.name}}</span>}@empty{<small>هنوز نقشی تعیین نشده</small>}</div>
        <div class="actions"><mat-form-field appearance="outline"><mat-label>افزودن نقش</mat-label><mat-select (selectionChange)="assignRole(user,$event.value)">@for(role of roles; track role.id){<mat-option [value]="role.id">{{role.name}}</mat-option>}</mat-select></mat-form-field>
        @for(role of user.roles; track role.id){<button mat-stroked-button type="button" (click)="removeRole(user,role.id)"><mat-icon>close</mat-icon>{{role.name}}</button>}</div>
      </mat-card>}@empty{<div class="empty">هنوز کاربری ثبت نشده است.</div>}</div>
    </section>`,
  styles:[`.page{max-width:1000px;margin:0 auto}.heading span{font-size:12px;color:#286f93;font-weight:700}.heading h1{margin:5px 0 6px;color:#102a35;font-size:25px}.heading p{color:#718087;margin:0 0 18px}.form-card{padding:18px;border-radius:18px;margin-bottom:14px}.form-card h2{font-size:16px;color:#102a35;margin:0 0 14px}form{display:grid;grid-template-columns:1fr 1fr 1fr auto;gap:10px;align-items:start}.form-card button{height:56px;background:#286f93;color:#fff}.list{display:grid;gap:10px}.user-card{padding:15px;border-radius:17px}.user-main{display:flex;align-items:center;gap:12px}.avatar{width:44px;height:44px;border-radius:50%;display:grid;place-items:center;background:#eaf3f7;color:#286f93}.identity{flex:1}.identity strong,.identity small{display:block}.identity strong{color:#102a35}.identity small{color:#879399;font-size:11px;margin-top:3px}.locked{font-size:11px;color:#a33;background:#fff0f0;padding:5px 8px;border-radius:9px}.roles{display:flex;flex-wrap:wrap;gap:6px;margin:12px 0}.roles span{background:#edf5f7;color:#286f93;border-radius:10px;padding:5px 9px;font-size:11px}.roles small{color:#89969b}.actions{display:flex;flex-wrap:wrap;gap:7px;align-items:center}.actions mat-form-field{width:210px}.actions button{height:44px}.error{margin-bottom:12px;padding:12px;border-radius:12px;background:#fff0f0;color:#a33}.empty{padding:28px;text-align:center;color:#8b989d;background:#fff;border-radius:16px}@media(max-width:800px){form{grid-template-columns:1fr}.form-card button{width:100%;height:50px}.actions{display:grid;grid-template-columns:1fr}.actions mat-form-field,.actions button{width:100%}.heading h1{font-size:21px}}`]
})
export class AdminUsersComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  users:User[]=[]; roles:Role[]=[]; saving=false; error='';
  form=this.fb.nonNullable.group({userName:['',[Validators.required,Validators.maxLength(100)]],email:['',[Validators.email]],password:['',[Validators.required,Validators.minLength(6)]]});
  constructor(){this.load();}
  load():void{
    this.http.get<User[]>('${environment.apiUrl}/identity/users').subscribe({next:r=>this.users=r,error:()=>this.error='دریافت کاربران انجام نشد.'});
    this.http.get<Role[]>('${environment.apiUrl}/identity/roles').subscribe({next:r=>this.roles=r,error:()=>this.error='دریافت نقش‌ها انجام نشد.'});
  }
  createUser():void{if(this.form.invalid)return;this.saving=true;this.http.post<User>('${environment.apiUrl}/identity/users',this.form.getRawValue()).subscribe({next:user=>{this.users=[...this.users,user];this.form.reset();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت کاربر انجام نشد.';this.saving=false}})}
  assignRole(user:User,roleId:string):void{if(!roleId)return;this.http.post('${environment.apiUrl}/identity/users/'+user.id+'/roles/'+roleId,{}).subscribe({next:()=>this.load(),error:e=>this.error=e.error?.detail||'افزودن نقش انجام نشد.'})}
  removeRole(user:User,roleId:string):void{this.http.delete('${environment.apiUrl}/identity/users/'+user.id+'/roles/'+roleId).subscribe({next:()=>this.load(),error:e=>this.error=e.error?.detail||'حذف نقش انجام نشد.'})}
}