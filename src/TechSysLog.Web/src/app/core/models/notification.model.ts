// ============================================================================
// Notification Domain Models
// ============================================================================

export type NotificationType = 'OrderCreated' | 'OrderStatusChanged' | 'OrderDelivered';

export interface Notification {
  readonly id: string;
  readonly userId: string;
  readonly type: NotificationType;
  readonly message: string;
  readonly data?: Record<string, unknown>;
  readonly read: boolean;
  readonly createdAt: string;
}

export interface NotificationListResponse {
  readonly success: boolean;
  readonly data: Notification[];
}

export interface UnreadCountResponse {
  readonly success: boolean;
  readonly data: {
    readonly count: number;
  };
}

// SignalR real-time events
export interface RealTimeNotification {
  readonly type: NotificationType;
  readonly message: string;
  readonly data?: Record<string, unknown>;
  readonly createdAt: string;
}

export interface OrderStatusChangedEvent {
  readonly orderId: string;
  readonly orderNumber: string;
  readonly userId: string;
  readonly oldStatus: string;
  readonly newStatus: string;
}

export interface UnreadCountUpdatedEvent {
  readonly count: number;
}

// Notification type display configuration
export interface NotificationTypeConfig {
  readonly label: string;
  readonly icon: string;
  readonly color: string;
}

export const NOTIFICATION_TYPE_CONFIG: Record<NotificationType, NotificationTypeConfig> = {
  OrderCreated: {
    label: 'Novo Pedido',
    icon: 'plus-circle',
    color: 'var(--color-primary-400)',
  },
  OrderStatusChanged: {
    label: 'Status Atualizado',
    icon: 'refresh-cw',
    color: 'var(--color-info)',
  },
  OrderDelivered: {
    label: 'Entrega Realizada',
    icon: 'package-check',
    color: 'var(--color-success)',
  },
};
