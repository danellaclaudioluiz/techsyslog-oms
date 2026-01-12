import { Injectable, inject, signal, computed, PLATFORM_ID, NgZone } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, tap, catchError, throwError, finalize, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { environment } from '@env/environment';
import {
  Notification,
  NotificationListResponse,
  UnreadCountResponse,
  RealTimeNotification,
  OrderStatusChangedEvent,
  UnreadCountUpdatedEvent,
} from '@core/models';
import { AuthService } from './auth.service';

// ============================================================================
// Notification Service
// Handles notifications and SignalR real-time connection
// ============================================================================

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  private readonly http = inject(HttpClient);
  private readonly authService = inject(AuthService);
  private readonly platformId = inject(PLATFORM_ID);
  private readonly ngZone = inject(NgZone);

  private readonly apiUrl = `${environment.apiUrl}/notifications`;
  private hubConnection: signalR.HubConnection | null = null;

  // === Reactive State ===
  private readonly _notifications = signal<Notification[]>([]);
  private readonly _unreadCount = signal(0);
  private readonly _isLoading = signal(false);
  private readonly _error = signal<string | null>(null);
  private readonly _isConnected = signal(false);

  // === Public State ===
  readonly notifications = this._notifications.asReadonly();
  readonly unreadCount = this._unreadCount.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly error = this._error.asReadonly();
  readonly isConnected = this._isConnected.asReadonly();

  readonly hasUnread = computed(() => this._unreadCount() > 0);

  // === Event Subjects for Components ===
  private readonly _newNotification = new Subject<RealTimeNotification>();
  private readonly _orderStatusChanged = new Subject<OrderStatusChangedEvent>();

  readonly newNotification$ = this._newNotification.asObservable();
  readonly orderStatusChanged$ = this._orderStatusChanged.asObservable();

  // === Public Methods ===

  loadNotifications(unreadOnly = false): Observable<NotificationListResponse> {
    this._isLoading.set(true);
    this._error.set(null);

    const url = unreadOnly ? `${this.apiUrl}?unreadOnly=true` : this.apiUrl;

    return this.http.get<NotificationListResponse>(url).pipe(
      tap((response) => {
        if (response.success) {
          this._notifications.set(response.data);
        }
      }),
      catchError((error) => {
        const message = error.error?.message || 'Erro ao carregar notificações.';
        this._error.set(message);
        return throwError(() => new Error(message));
      }),
      finalize(() => this._isLoading.set(false))
    );
  }

  loadUnreadCount(): Observable<UnreadCountResponse> {
    return this.http.get<UnreadCountResponse>(`${this.apiUrl}/unread-count`).pipe(
      tap((response) => {
        if (response.success) {
          this._unreadCount.set(response.data.count);
        }
      }),
      catchError((error) => {
        console.error('Error loading unread count:', error);
        return throwError(() => error);
      })
    );
  }

  markAsRead(notificationId: string): Observable<unknown> {
    return this.http.patch(`${this.apiUrl}/${notificationId}/read`, {}).pipe(
      tap(() => {
        // Update local state
        this._notifications.update(current =>
          current.map(n =>
            n.id === notificationId ? { ...n, read: true } : n
          )
        );
        this._unreadCount.update(count => Math.max(0, count - 1));
      }),
      catchError((error) => {
        console.error('Error marking notification as read:', error);
        return throwError(() => error);
      })
    );
  }

  markAllAsRead(): Observable<unknown> {
    return this.http.patch(`${this.apiUrl}/read-all`, {}).pipe(
      tap(() => {
        // Update local state
        this._notifications.update(current =>
          current.map(n => ({ ...n, read: true }))
        );
        this._unreadCount.set(0);
      }),
      catchError((error) => {
        console.error('Error marking all as read:', error);
        return throwError(() => error);
      })
    );
  }

  // === SignalR Connection ===

  async startConnection(): Promise<void> {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const token = this.authService.token();
    if (!token) {
      console.warn('Cannot start SignalR connection without token');
      return;
    }

    // Don't reconnect if already connected
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      this.hubConnection = new signalR.HubConnectionBuilder()
        .withUrl(environment.signalRUrl, {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            // Exponential backoff: 0, 2s, 4s, 8s, 16s, then cap at 30s
            if (retryContext.previousRetryCount < 5) {
              return Math.pow(2, retryContext.previousRetryCount) * 1000;
            }
            return 30000;
          },
        })
        .configureLogging(signalR.LogLevel.Warning)
        .build();

      this.registerEventHandlers();
      await this.hubConnection.start();
      this.ngZone.run(() => {
        this._isConnected.set(true);
      });

      console.log('SignalR connected');
    } catch (error) {
      console.error('SignalR connection error:', error);
      this.ngZone.run(() => {
        this._isConnected.set(false);
        console.error('SignalR connection error:', error);
      });
    }
  }

  async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        this._isConnected.set(false);
        console.log('SignalR disconnected');
      } catch (error) {
        console.error('Error stopping SignalR:', error);
      }
    }
  }

  // === Private Methods ===

  private registerEventHandlers(): void {
  if (!this.hubConnection) {
    return;
  }

  // Handle new notification
  this.hubConnection.on('ReceiveNotification', (notification: RealTimeNotification) => {
    this.ngZone.run(() => {
      this._newNotification.next(notification);
      this._unreadCount.update(count => count + 1);
    });
  });

  // Handle order status change
  this.hubConnection.on('OrderStatusChanged', (event: OrderStatusChangedEvent) => {
    this.ngZone.run(() => {
      this._orderStatusChanged.next(event);
    });
  });

  // Handle unread count update
  this.hubConnection.on('UnreadCountUpdated', (event: UnreadCountUpdatedEvent) => {
    this.ngZone.run(() => {
      this._unreadCount.set(event.count);
    });
  });

  // Handle reconnection
  this.hubConnection.onreconnecting(() => {
    this.ngZone.run(() => {
      this._isConnected.set(false);
    });
  });

  this.hubConnection.onreconnected(() => {
    this.ngZone.run(() => {
      this._isConnected.set(true);
      this.loadUnreadCount().subscribe();
    });
  });

  this.hubConnection.onclose(() => {
    this.ngZone.run(() => {
      this._isConnected.set(false);
    });
  });
}

  clearNotifications(): void {
    this._notifications.set([]);
    this._unreadCount.set(0);
  }

  clearError(): void {
    this._error.set(null);
  }
}
