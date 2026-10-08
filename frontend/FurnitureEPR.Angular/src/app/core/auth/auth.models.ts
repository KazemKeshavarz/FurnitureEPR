export interface LoginRequest {
  userName: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
}

export interface CurrentUser {
  userId: string | null;
  userName: string | null;
  roles: string[];
  roleIds: string[];
}

export interface JwtPayload {
  sub?: string;
  nameid?: string;
  unique_name?: string;
  name?: string;
  role?: string | string[];
  role_id?: string | string[];
  permission?: string | string[];
  exp?: number;
  [key: string]: unknown;
}
