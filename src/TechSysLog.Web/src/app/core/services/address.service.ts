import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, finalize, of, debounceTime, distinctUntilChanged, switchMap } from 'rxjs';
import { environment } from '@env/environment';
import { AddressLookup, AddressLookupResponse } from '@core/models';

// ============================================================================
// Address Service
// Handles CEP lookup via API
// ============================================================================

@Injectable({
  providedIn: 'root',
})
export class AddressService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/address`;

  // === Reactive State ===
  private readonly _currentAddress = signal<AddressLookup | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // === Public State ===
  readonly currentAddress = this._currentAddress.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // Cache to avoid repeated API calls
  private readonly cache = new Map<string, AddressLookup>();

  // === Public Methods ===

  lookupCep(cep: string): Observable<AddressLookupResponse> {
    // Normalize CEP (remove non-digits)
    const normalizedCep = cep.replace(/\D/g, '');

    // Validate CEP format
    if (normalizedCep.length !== 8) {
      const error = 'CEP deve ter 8 dígitos';
      this._error.set(error);
      return throwError(() => new Error(error));
    }

    // Check cache first
    const cached = this.cache.get(normalizedCep);
    if (cached) {
      this._currentAddress.set(cached);
      return of({ success: true, data: cached });
    }

    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<AddressLookupResponse>(`${this.apiUrl}/${normalizedCep}`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this._currentAddress.set(response.data);
          // Cache the result
          this.cache.set(normalizedCep, response.data);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'CEP não encontrado. Verifique e tente novamente.';
        this._error.set(message);
        this._currentAddress.set(null);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  clearAddress(): void {
    this._currentAddress.set(null);
    this._error.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }

  // === Helper Methods ===

  formatCep(cep: string): string {
    const digits = cep.replace(/\D/g, '');
    if (digits.length <= 5) {
      return digits;
    }
    return `${digits.slice(0, 5)}-${digits.slice(5, 8)}`;
  }

  isValidCep(cep: string): boolean {
    const digits = cep.replace(/\D/g, '');
    return digits.length === 8;
  }
}
