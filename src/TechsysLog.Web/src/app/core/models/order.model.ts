// ============================================================================
// Order Domain Models
// ============================================================================

export type OrderStatus = 'Pending' | 'Confirmed' | 'InTransit' | 'Delivered' | 'Cancelled';

export interface Address {
  readonly cep: string;
  readonly cepFormatted?: string;
  readonly street: string;
  readonly number: string;
  readonly complement?: string;
  readonly neighborhood: string;
  readonly city: string;
  readonly state: string;
}

export interface Order {
  readonly id: string;
  readonly orderNumber: string;
  readonly description: string;
  readonly value: number;
  readonly status: OrderStatus | number;
  readonly deliveryAddress: Address;
  readonly userId: string;
  readonly createdAt: string;
  readonly updatedAt?: string | null;
  readonly deliveredAt?: string | null;
}

// Mapeamento de número para status
export const STATUS_MAP: Record<number, OrderStatus> = {
  1: 'Pending',
  2: 'Confirmed',
  3: 'InTransit',
  4: 'Delivered',
  5: 'Cancelled',
};

// Mapeamento de status para número
export const STATUS_TO_NUMBER: Record<OrderStatus, number> = {
  'Pending': 1,
  'Confirmed': 2,
  'InTransit': 3,
  'Delivered': 4,
  'Cancelled': 5,
};

export function getStatusFromNumber(status: OrderStatus | number): OrderStatus {
  if (typeof status === 'number') {
    return STATUS_MAP[status] || 'Pending';
  }
  return status;
}

export interface CreateOrderRequest {
  readonly description: string;
  readonly value: number;
  readonly cep: string;
  readonly number: string;
  readonly complement?: string;
}

export interface UpdateOrderStatusRequest {
  readonly status: OrderStatus | number;
}

export interface OrderListResponse {
  readonly success: boolean;
  readonly data: Order[];
  readonly pagination?: {
    readonly cursor?: string;
    readonly hasMore: boolean;
    readonly limit: number;
  };
}

export interface OrderResponse {
  readonly success: boolean;
  readonly data?: Order;
  readonly message?: string;
}

export interface OrderFormValue {
  description: string;
  value: number;
  cep: string;
  number: string;
  complement: string;
}

export interface StatusConfig {
  readonly label: string;
  readonly color: string;
  readonly bgColor: string;
  readonly icon: 'clock' | 'check-circle' | 'truck' | 'package-check' | 'x-circle';
}

export const ORDER_STATUS_CONFIG: Record<OrderStatus, StatusConfig> = {
  Pending: {
    label: 'Pendente',
    color: 'var(--color-status-pending)',
    bgColor: 'rgba(245, 158, 11, 0.15)',
    icon: 'clock',
  },
  Confirmed: {
    label: 'Confirmado',
    color: 'var(--color-status-confirmed)',
    bgColor: 'rgba(59, 130, 246, 0.15)',
    icon: 'check-circle',
  },
  InTransit: {
    label: 'Em Trânsito',
    color: 'var(--color-status-in-transit)',
    bgColor: 'rgba(139, 92, 246, 0.15)',
    icon: 'truck',
  },
  Delivered: {
    label: 'Entregue',
    color: 'var(--color-status-delivered)',
    bgColor: 'rgba(16, 185, 129, 0.15)',
    icon: 'package-check',
  },
  Cancelled: {
    label: 'Cancelado',
    color: 'var(--color-status-cancelled)',
    bgColor: 'rgba(239, 68, 68, 0.15)',
    icon: 'x-circle',
  },
};

export const STATUS_TRANSITIONS: Record<OrderStatus, OrderStatus[]> = {
  Pending: ['Confirmed', 'Cancelled'],
  Confirmed: ['InTransit', 'Cancelled'],
  InTransit: ['Delivered'],
  Delivered: [],
  Cancelled: [],
};