import { Injectable, signal, computed, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap, catchError, throwError, finalize } from 'rxjs';
import { environment } from '@env/environment';
import {
  User,
  AuthUser,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  UserRole,
} from '@core/models';

// ============================================================================
// Auth Service
// Handles authentication state and API calls
// Uses signals for reactive state management
// ============================================================================

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly platformId = inject(PLATFORM_ID);

  private readonly apiUrl = `${environment.apiUrl}/auth`;

  // === Reactive State ===
  private readonly _user = signal<User | null>(null);
  private readonly _token = signal<string | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // === Public Computed State ===
  readonly user = this._user.asReadonly();
  readonly token = this._token.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  readonly isAuthenticated = computed(() => !!this._token());
  
  readonly isAdmin = computed(() => {
    const role = this._user()?.role;
    return role === 'Admin' || role === 3;
  });
  
  readonly isOperator = computed(() => {
    const role = this._user()?.role;
    return role === 'Operator' || role === 2;
  });
  
  readonly isCustomer = computed(() => {
    const role = this._user()?.role;
    return role === 'Customer' || role === 1;
  });

  readonly canManageUsers = computed(() => {
    const role = this._user()?.role;
    return role === 'Admin' || role === 3;
  });

  constructor() {
    this.loadStoredAuth();
  }

  // === Public Methods ===

  canManageOrders(): boolean {
    const user = this.user();
    if (!user) return false;
    
    const role = user.role;
    return role === 'Admin' || role === 'Operator' || role === 3 || role === 2;
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.setAuth(response.data.user, response.data.token);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao fazer login. Verifique suas credenciais.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  register(userData: RegisterRequest): Observable<AuthResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, userData).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.setAuth(response.data.user, response.data.token);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao criar conta. Tente novamente.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  refreshToken(): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/refresh`, {}).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this.setAuth(response.data.user, response.data.token);
        }
      }),
      catchError((error) => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  getCurrentUser(): Observable<AuthResponse> {
    return this.http.get<AuthResponse>(`${this.apiUrl}/me`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this._user.set(response.data.user);
        }
      }),
      catchError((error) => {
        if (error.status === 401) {
          this.logout();
        }
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.clearAuth();
    this.router.navigate(['/auth/login']);
  }

  clearError(): void {
    this._error.set(null);
  }

  hasRole(role: UserRole | number): boolean {
    const userRole = this._user()?.role;
    return userRole === role;
  }

  hasAnyRole(roles: (UserRole | number)[]): boolean {
    const userRole = this._user()?.role;
    if (!userRole) return false;
    
    return roles.some(role => {
      if (typeof role === 'number' && typeof userRole === 'number') {
        return role === userRole;
      }
      if (typeof role === 'string' && typeof userRole === 'string') {
        return role === userRole;
      }
      const roleMap: Record<number, string> = { 1: 'Customer', 2: 'Operator', 3: 'Admin' };
      if (typeof role === 'string' && typeof userRole === 'number') {
        return role === roleMap[userRole];
      }
      if (typeof role === 'number' && typeof userRole === 'string') {
        return roleMap[role] === userRole;
      }
      return false;
    });
  }

  // === Private Methods ===

  private setAuth(user: User, token: string): void {
    this._user.set(user);
    this._token.set(token);
    this.persistAuth(user, token);
  }

  private clearAuth(): void {
    this._user.set(null);
    this._token.set(null);
    this._error.set(null);
    this.clearStoredAuth();
  }

  private loadStoredAuth(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    try {
      const token = localStorage.getItem(environment.tokenKey);
      const userJson = localStorage.getItem(environment.userKey);

      if (token && userJson) {
        const user = JSON.parse(userJson) as User;
        this._user.set(user);
        this._token.set(token);
      }
    } catch (error) {
      console.error('Error loading stored auth:', error);
      this.clearStoredAuth();
    }
  }

  private persistAuth(user: User, token: string): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    try {
      localStorage.setItem(environment.tokenKey, token);
      localStorage.setItem(environment.userKey, JSON.stringify(user));
    } catch (error) {
      console.error('Error persisting auth:', error);
    }
  }

  private clearStoredAuth(): void {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    try {
      localStorage.removeItem(environment.tokenKey);
      localStorage.removeItem(environment.userKey);
    } catch (error) {
      console.error('Error clearing stored auth:', error);
    }
  }
}