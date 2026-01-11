import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap, catchError, throwError, finalize } from 'rxjs';
import { environment } from '@env/environment';
import {
  Order,
  OrderStatus,
  CreateOrderRequest,
  UpdateOrderStatusRequest,
  STATUS_TRANSITIONS,
  getStatusFromNumber,
} from '@core/models';

// ============================================================================
// Order Service
// Handles order management and state
// ============================================================================

export interface OrderFilters {
  status?: OrderStatus;
  limit?: number;
  cursor?: string;
}

@Injectable({
  providedIn: 'root',
})
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/Orders`;

  // === Reactive State ===
  private readonly _orders = signal<Order[]>([]);
  private readonly _selectedOrder = signal<Order | null>(null);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _pagination = signal<{
    cursor?: string | null;
    hasMore: boolean;
    limit: number;
  } | null>(null);

  // === Public Computed State ===
  readonly orders = this._orders.asReadonly();
  readonly selectedOrder = this._selectedOrder.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly pagination = this._pagination.asReadonly();
  readonly hasMore = computed(() => this._pagination()?.hasMore ?? false);

  // Order statistics
  readonly orderStats = computed(() => {
    const orders = this._orders();
    return {
      total: orders.length,
      pending: orders.filter(o => getStatusFromNumber(o.status) === 'Pending').length,
      confirmed: orders.filter(o => getStatusFromNumber(o.status) === 'Confirmed').length,
      inTransit: orders.filter(o => getStatusFromNumber(o.status) === 'InTransit').length,
      delivered: orders.filter(o => getStatusFromNumber(o.status) === 'Delivered').length,
      cancelled: orders.filter(o => getStatusFromNumber(o.status) === 'Cancelled').length,
    };
  });

  // === Public Methods ===

  loadOrders(params: OrderFilters = {}): Observable<any> {
    this._isLoading.set(true);

    let httpParams = new HttpParams();
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.limit) httpParams = httpParams.set('limit', params.limit.toString());
    if (params.cursor) httpParams = httpParams.set('cursor', params.cursor);

    return this.http.get<any>(this.apiUrl, { params: httpParams }).pipe(
      tap((response: any) => {
        // API retorna { success, data: { data: [...], cursor, hasMore, limit, totalCount } }
        const result = response.data;
        const orders = result?.data || result || [];
        
        if (Array.isArray(orders)) {
          if (params.cursor) {
            this._orders.update(current => [...current, ...orders]);
          } else {
            this._orders.set(orders);
          }
        } else {
          if (!params.cursor) {
            this._orders.set([]);
          }
        }
        
        // Pagination
        this._pagination.set({
          cursor: result?.cursor || null,
          hasMore: result?.hasMore || false,
          limit: result?.limit || 20,
        });
        
        this._isLoading.set(false);
      }),
      catchError(error => {
        console.error('Error loading orders:', error);
        this._isLoading.set(false);
        if (!params.cursor) {
          this._orders.set([]);
        }
        return throwError(() => error);
      })
    );
  }

  loadOrderById(id: string): Observable<any> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.get<any>(`${this.apiUrl}/${id}`).pipe(
      tap((response: any) => {
        if (response.success && response.data) {
          this._selectedOrder.set(response.data);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao carregar pedido.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  createOrder(orderData: CreateOrderRequest): Observable<any> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.post<any>(this.apiUrl, orderData).pipe(
      tap((response: any) => {
        if (response.success && response.data) {
          // Add new order to the beginning of the list
          this._orders.update(current => [response.data, ...current]);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao criar pedido.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  updateOrderStatus(id: string, status: OrderStatus): Observable<any> {
    this._isLoading.set(true);
    this._error.set(null);

    const request: UpdateOrderStatusRequest = { status };

    return this.http.patch<any>(`${this.apiUrl}/${id}/status`, request).pipe(
      tap((response: any) => {
        if (response.success && response.data) {
          // Update order in list
          this._orders.update(current =>
            current.map(order =>
              order.id === id ? response.data : order
            )
          );
          // Update selected order if it's the same
          if (this._selectedOrder()?.id === id) {
            this._selectedOrder.set(response.data);
          }
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao atualizar status do pedido.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  deleteOrder(id: string): Observable<any> {
    this._isLoading.set(true);
    this._error.set(null);

    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(
      tap((response: any) => {
        if (response.success) {
          // Remove from list
          this._orders.update(current =>
            current.filter(order => order.id !== id)
          );
          // Clear selected if it was deleted
          if (this._selectedOrder()?.id === id) {
            this._selectedOrder.set(null);
          }
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao excluir pedido.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  // === Helper Methods ===

  getValidTransitions(currentStatus: OrderStatus): OrderStatus[] {
    return STATUS_TRANSITIONS[currentStatus] || [];
  }

  canTransitionTo(currentStatus: OrderStatus, newStatus: OrderStatus): boolean {
    return STATUS_TRANSITIONS[currentStatus]?.includes(newStatus) ?? false;
  }

  selectOrder(order: Order | null): void {
    this._selectedOrder.set(order);
  }

  clearError(): void {
    this._error.set(null);
  }

  clearOrders(): void {
    this._orders.set([]);
    this._selectedOrder.set(null);
    this._pagination.set(null);
  }

  // Update order in local state (for real-time updates)
  updateOrderLocally(updatedOrder: Order): void {
    this._orders.update(current =>
      current.map(order =>
        order.id === updatedOrder.id ? updatedOrder : order
      )
    );
    if (this._selectedOrder()?.id === updatedOrder.id) {
      this._selectedOrder.set(updatedOrder);
    }
  }

  // Add order to local state (for real-time updates)
  addOrderLocally(newOrder: Order): void {
    this._orders.update(current => [newOrder, ...current]);
  }
}