import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule
  ],
  template: `
    <main class="login-page">
      <section class="login-brand">
        <div class="brand-mark">E</div>
        <span class="eyebrow">Furniture EPR</span>
        <h1>مدیریت هوشمند تولید و سفارش</h1>
        <p>مدیریت سفارش‌ها، فرآیند تولید و کنترل کیفیت در یک پنل یکپارچه.</p>
      </section>

      <mat-card class="login-card">
        <mat-card-header>
          <mat-card-title>ورود به سامانه</mat-card-title>
          <mat-card-subtitle>اطلاعات حساب کاربری خود را وارد کنید.</mat-card-subtitle>
        </mat-card-header>

        <mat-card-content>
          <form [formGroup]="form" (ngSubmit)="submit()">
            <mat-form-field appearance="outline">
              <mat-label>نام کاربری</mat-label>
              <input matInput formControlName="userName" autocomplete="username">
              @if (form.controls.userName.hasError('required') && form.controls.userName.touched) {
                <mat-error>نام کاربری الزامی است.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline">
              <mat-label>رمز عبور</mat-label>
              <input matInput type="password" formControlName="password" autocomplete="current-password">
              @if (form.controls.password.hasError('required') && form.controls.password.touched) {
                <mat-error>رمز عبور الزامی است.</mat-error>
              }
            </mat-form-field>

            <button mat-flat-button color="primary" type="submit" [disabled]="form.invalid">
              ورود
            </button>
          </form>
        </mat-card-content>
      </mat-card>
    </main>
  `,
  styles: [`
    :host { display: block; min-height: 100vh; }
    .login-page {
      min-height: 100vh;
      display: grid;
      grid-template-columns: 1.15fr .85fr;
      align-items: center;
      gap: 64px;
      padding: 48px clamp(24px, 7vw, 120px);
      box-sizing: border-box;
      background: radial-gradient(circle at 15% 20%, rgba(48, 112, 142, .12), transparent 32%), #f7f9fa;
    }
    .login-brand { max-width: 620px; }
    .brand-mark {
      width: 58px; height: 58px; border-radius: 18px;
      display: grid; place-items: center; margin-bottom: 24px;
      background: #286f93; color: white; font-size: 28px; font-weight: 700;
      box-shadow: 0 14px 32px rgba(40,111,147,.24);
    }
    .eyebrow { color: #286f93; font-weight: 700; letter-spacing: .04em; }
    h1 { margin: 12px 0 18px; font-size: clamp(32px, 4vw, 54px); line-height: 1.25; color: #102a35; }
    p { margin: 0; color: #64747b; font-size: 18px; line-height: 2; }
    .login-card { width: min(100%, 430px); padding: 12px; border-radius: 24px; }
    mat-card-header { margin-bottom: 18px; }
    mat-card-title { font-size: 24px; color: #102a35; }
    mat-card-subtitle { margin-top: 8px; }
    form { display: grid; gap: 12px; }
    button { height: 50px; border-radius: 12px; font-size: 16px; }
    @media (max-width: 800px) {
      .login-page { grid-template-columns: 1fr; gap: 28px; padding: 28px 18px; }
      .login-brand { text-align: center; margin: auto; }
      .brand-mark { margin-inline: auto; }
      .login-card { margin: auto; }
    }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);

  readonly form = this.fb.nonNullable.group({
    userName: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    // در مرحله بعد این فرم به API احراز هویت متصل می‌شود.
  }
}
