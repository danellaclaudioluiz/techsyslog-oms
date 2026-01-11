// ============================================================================
// Delivery Domain Models
// ============================================================================

export interface Delivery {
  readonly id: string;
  readonly orderId: string;
  readonly orderNumber: string;
  readonly deliveredAt: string;
  readonly deliveredBy: string;
  readonly createdAt: string;
}

export interface CreateDeliveryRequest {
  readonly orderId: string;
}

export interface DeliveryResponse {
  readonly success: boolean;
  readonly data?: Delivery;
  readonly message?: string;
}
