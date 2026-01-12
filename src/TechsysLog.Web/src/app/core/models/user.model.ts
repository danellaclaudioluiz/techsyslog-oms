// ============================================================================
// User Domain Models
// ============================================================================

export type UserRole = 'Admin' | 'Operator' | 'Customer';

export interface User {
  readonly id: string;
  readonly name: string;
  readonly email: string;
  readonly role: UserRole | number;
  readonly createdAt: string;
  readonly updatedAt?: string;
}

export interface AuthUser extends User {
  readonly token: string;
}

export interface LoginRequest {
  readonly email: string;
  readonly password: string;
}

export interface RegisterRequest {
  readonly name: string;
  readonly email: string;
  readonly password: string;
  readonly role: UserRole;
}

export interface AuthResponse {
  readonly success: boolean;
  readonly data?: {
    readonly token: string;
    readonly user: User;
  };
  readonly message?: string;
}

export interface LoginFormValue {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface RegisterFormValue {
  name: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: UserRole;
}
