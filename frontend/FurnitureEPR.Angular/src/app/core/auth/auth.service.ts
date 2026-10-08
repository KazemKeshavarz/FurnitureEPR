import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CurrentUser, JwtPayload, LoginRequest, LoginResponse } from './auth.models';

const TOKEN_KEY = 'furniture_epr_access_token';
const EXPIRATION_KEY = 'furniture_epr_token_expiration';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(response => this.storeToken(response))
    );
  }

  me(): Observable<CurrentUser> {
    return this.http.get<CurrentUser>(`${environment.apiUrl}/auth/me`);
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRATION_KEY);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    const expiration = Number(localStorage.getItem(EXPIRATION_KEY));
    return Number.isFinite(expiration) && expiration > Date.now();
  }

  getPermissions(): string[] {
    const payload = this.readPayload();
    return this.toStringArray(payload?.permission);
  }

  getRoles(): string[] {
    const payload = this.readPayload();
    return this.toStringArray(payload?.role);
  }

  hasPermission(permission: string): boolean {
    return this.getPermissions().includes(permission);
  }

  private storeToken(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(EXPIRATION_KEY, String(new Date(response.expiresAtUtc).getTime()));
  }

  private readPayload(): JwtPayload | null {
    const token = this.getToken();
    if (!token) {
      return null;
    }

    try {
      const payload = token.split('.')[1];
      const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
      const decoded = decodeURIComponent(
        atob(normalized)
          .split('')
          .map(character => `%${('00' + character.charCodeAt(0).toString(16)).slice(-2)}`)
          .join('')
      );

      return JSON.parse(decoded) as JwtPayload;
    } catch {
      return null;
    }
  }

  private toStringArray(value: string | string[] | undefined): string[] {
    if (Array.isArray(value)) {
      return value;
    }

    return value ? [value] : [];
  }
}
