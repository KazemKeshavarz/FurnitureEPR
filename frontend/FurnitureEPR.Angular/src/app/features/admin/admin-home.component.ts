import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
@Component({
 selector:'app-admin-home',standalone:true,imports:[RouterLink,MatCardModule,MatIconModule],
 template:`<section class="page"><span>مدیریت سامانه</span><h1>تعاریف و مدیریت</h1><p>اطلاعات پایه و تنظیمات سامانه را از این بخش مدیریت کنید.</p><div class="cards">
 <a mat-card routerLink="/admin/master-data"><mat-icon>category</mat-icon><strong>تعاریف محصول</strong><small>دسته‌بندی، محصول و اجزای محصول</small></a>
 <a mat-card routerLink="/admin/workflows"><mat-icon>account_tree</mat-icon><strong>فرآیندهای تولید</strong><small>تعریف مراحل و مسیر انجام کار</small></a>
 <a mat-card routerLink="/admin/users"><mat-icon>manage_accounts</mat-icon><strong>کاربران</strong><small>مدیریت کاربران سامانه</small></a>
 <a mat-card routerLink="/admin/roles"><mat-icon>admin_panel_settings</mat-icon><strong>نقش‌ها و دسترسی‌ها</strong><small>تعیین سطح دسترسی</small></a>
 </div></section>`,
 styles:[`.page{max-width:1000px;margin:0 auto}.page>span{color:#286f93;font-size:12px;font-weight:700}h1{margin:5px 0 8px;color:#102a35;font-size:25px}.page>p{color:#718087;margin-bottom:22px}.cards{display:grid;grid-template-columns:repeat(2,1fr);gap:14px}.cards a{padding:20px;border-radius:18px;text-decoration:none;display:block}.cards mat-icon{color:#286f93;font-size:30px;width:30px;height:30px}.cards strong,.cards small{display:block}.cards strong{margin-top:15px;color:#102a35;font-size:16px}.cards small{margin-top:6px;color:#78868c;line-height:1.8}@media(max-width:700px){.cards{grid-template-columns:1fr}h1{font-size:21px}.cards a{padding:18px}}`]
}) export class AdminHomeComponent {}