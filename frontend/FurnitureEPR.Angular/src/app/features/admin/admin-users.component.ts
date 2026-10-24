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
  templateUrl: './admin-users.component.html',
  styleUrl: './admin-users.component.scss'
})
export class AdminUsersComponent {
  private readonly http=inject(HttpClient); private readonly fb=inject(FormBuilder);
  users:User[]=[]; roles:Role[]=[]; saving=false; error='';
  form=this.fb.nonNullable.group({userName:['',[Validators.required,Validators.maxLength(100)]],email:['',[Validators.email]],password:['',[Validators.required,Validators.minLength(6)]]});
  constructor(){this.load();}
  load():void{
    this.http.get<User[]>(`${environment.apiUrl}/identity/users`).subscribe({next:r=>this.users=r,error:()=>this.error='دریافت کاربران انجام نشد.'});
    this.http.get<Role[]>(`${environment.apiUrl}/identity/roles`).subscribe({next:r=>this.roles=r,error:()=>this.error='دریافت نقش‌ها انجام نشد.'});
  }
  createUser():void{if(this.form.invalid)return;this.saving=true;this.http.post<User>(`${environment.apiUrl}/identity/users`,this.form.getRawValue()).subscribe({next:user=>{this.users=[...this.users,user];this.form.reset();this.saving=false},error:e=>{this.error=e.error?.detail||'ثبت کاربر انجام نشد.';this.saving=false}})}
  assignRole(user:User,roleId:string):void{if(!roleId)return;this.http.post(`${environment.apiUrl}/identity/users/`+user.id+'/roles/'+roleId,{}).subscribe({next:()=>this.load(),error:e=>this.error=e.error?.detail||'افزودن نقش انجام نشد.'})}
  removeRole(user:User,roleId:string):void{this.http.delete(`${environment.apiUrl}/identity/users/`+user.id+'/roles/'+roleId).subscribe({next:()=>this.load(),error:e=>this.error=e.error?.detail||'حذف نقش انجام نشد.'})}
}