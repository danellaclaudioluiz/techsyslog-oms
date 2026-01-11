import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError, finalize } from 'rxjs';
import { environment } from '@env/environment';
import { Delivery, CreateDeliveryRequest, DeliveryResponse } from '@core/models';

// ============================================================================
// Delivery Service
// Handles delivery registration and queries
// ============================================================================

@Injectable({
  providedIn: 'root',
})
export class DeliveryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/deliveries`;

  // === Reactive State ===
  private readonly _currentDelivery = signal<Delivery | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);

  // === Public State ===
  readonly currentDelivery = this._currentDelivery.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();

  // === Public Methods ===

  registerDelivery(orderId: string): Observable<DeliveryResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    const request: CreateDeliveryRequest = { orderId };

    return this.http.post<DeliveryResponse>(this.apiUrl, request).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this._currentDelivery.set(response.data);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao registrar entrega. O pedido deve estar em trânsito.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  getDeliveryByOrderId(orderId: string): Observable<DeliveryResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<DeliveryResponse>(`${this.apiUrl}/${orderId}`).pipe(
      tap((response) => {
        if (response.success && response.data) {
          this._currentDelivery.set(response.data);
        }
      }),
      catchError((error) => {
        // 404 is expected if no delivery exists
        if (error.status === 404) {
          this._currentDelivery.set(null);
          return throwError(() => new Error('Entrega não encontrada'));
        }
        const message = error.error?.message || 'Erro ao buscar entrega.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  clearDelivery(): void {
    this._currentDelivery.set(null);
    this._error.set(null);
  }

  clearError(): void {
    this._error.set(null);
  }
}
