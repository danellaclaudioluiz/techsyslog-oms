import { Component, ChangeDetectionStrategy, inject, OnInit, signal } from '@angular/core';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { OrderService, DeliveryService, AuthService, ToastService } from '@core/services';
import { Order, OrderStatus, ORDER_STATUS_CONFIG, STATUS_TRANSITIONS, getStatusFromNumber } from '@core/models';
import { 
  CardComponent, 
  IconComponent, 
  ButtonComponent,
  OrderStatusBadgeComponent,
  LoaderComponent,
} from '@shared/components/ui';

@Component({
  selector: 'app-order-detail-page',
  standalone: true,
  imports: [
    RouterLink,
    CardComponent,
    IconComponent,
    ButtonComponent,
    OrderStatusBadgeComponent,
    LoaderComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="order-detail-page">
      @if (orderService.isLoading() && !order()) {
        <div class="loading-state">
          <app-loader size="lg" text="Carregando pedido..." />
        </div>
      } @else if (order(); as order) {
        <header class="page-header">
          <a routerLink="/orders" class="back-link">
            <app-icon name="arrow-left" [size]="20" />
            <span>Voltar para pedidos</span>
          </a>
          <div class="header-content">
            <div class="header-info">
              <h1 class="page-title">{{ order.orderNumber }}</h1>
              <app-order-status-badge [status]="getStatus(order)" />
            </div>
            <p class="page-subtitle">Criado em {{ formatDate(order.createdAt) }}</p>
          </div>
        </header>
        
        <!-- Status Timeline -->
        <section class="timeline-section">
          <app-card title="Status do Pedido" class="timeline-card">
            <div class="status-timeline">
              @for (status of statusTimeline; track status.status; let i = $index) {
                <div 
                  class="timeline-item"
                  [class.active]="isStatusActive(status.status, getStatus(order))"
                  [class.current]="status.status === getStatus(order)"
                  [class.completed]="isStatusCompleted(status.status, getStatus(order))"
                >
                  <div class="timeline-marker">
                    @if (isStatusCompleted(status.status, getStatus(order))) {
                      <app-icon name="check" [size]="14" />
                    } @else {
                      <span class="marker-number">{{ i + 1 }}</span>
                    }
                  </div>
                  <div class="timeline-content">
                    <span class="timeline-label">{{ status.label }}</span>
                  </div>
                  @if (i < statusTimeline.length - 1) {
                    <div class="timeline-connector"></div>
                  }
                </div>
              }
            </div>
            
            @if (authService.canManageOrders() && getValidTransitions(getStatus(order)).length > 0) {
              <div class="status-actions">
                @for (nextStatus of getValidTransitions(getStatus(order)); track nextStatus) {
                  <app-button
                    [variant]="nextStatus === 'Cancelled' ? 'danger' : 'primary'"
                    size="sm"
                    [loading]="updatingStatus()"
                    (clicked)="updateStatus(order.id, nextStatus)"
                  >
                    @if (nextStatus === 'Cancelled') {
                      Cancelar Pedido
                    } @else {
                      Avançar para {{ ORDER_STATUS_CONFIG[nextStatus].label }}
                    }
                  </app-button>
                }
              </div>
            }
          </app-card>
        </section>
        
        <div class="content-grid">
          <!-- Order Info -->
          <app-card title="Detalhes do Pedido" class="info-card">
            <div class="info-grid">
              <div class="info-item">
                <app-icon name="file-text" [size]="18" />
                <div class="info-content">
                  <span class="info-label">Descrição</span>
                  <span class="info-value">{{ order.description }}</span>
                </div>
              </div>
              
              <div class="info-item">
                <app-icon name="dollar-sign" [size]="18" />
                <div class="info-content">
                  <span class="info-label">Valor</span>
                  <span class="info-value highlight">{{ formatCurrency(order.value) }}</span>
                </div>
              </div>
              
              <div class="info-item">
                <app-icon name="calendar" [size]="18" />
                <div class="info-content">
                  <span class="info-label">Data de Criação</span>
                  <span class="info-value">{{ formatDateTime(order.createdAt) }}</span>
                </div>
              </div>
              
              @if (order.deliveredAt) {
                <div class="info-item">
                  <app-icon name="package-check" [size]="18" />
                  <div class="info-content">
                    <span class="info-label">Data de Entrega</span>
                    <span class="info-value success">{{ formatDateTime(order.deliveredAt) }}</span>
                  </div>
                </div>
              }
            </div>
          </app-card>
          
          <!-- Address -->
          <app-card title="Endereço de Entrega" class="address-card">
            @if (order.deliveryAddress) {
              <div class="address-content">
                <div class="address-icon">
                  <app-icon name="map-pin" [size]="24" />
                </div>
                <div class="address-details">
                  <span class="address-street">
                    {{ order.deliveryAddress.street }}, {{ order.deliveryAddress.number }}
                    @if (order.deliveryAddress.complement) {
                      - {{ order.deliveryAddress.complement }}
                    }
                  </span>
                  <span class="address-neighborhood">{{ order.deliveryAddress.neighborhood }}</span>
                  <span class="address-city">
                    {{ order.deliveryAddress.city }} - {{ order.deliveryAddress.state }}
                  </span>
                  <span class="address-cep">CEP: {{ order.deliveryAddress.cepFormatted || formatCep(order.deliveryAddress.cep) }}</span>
                </div>
              </div>
            } @else {
              <div class="address-empty">
                <app-icon name="map-pin" [size]="32" />
                <span>Endereço não disponível</span>
              </div>
            }
          </app-card>
        </div>
        
        <!-- Actions -->
        @if (getStatus(order) === 'InTransit' && authService.canManageOrders()) {
          <section class="actions-section">
            <app-card class="action-card">
              <div class="action-content">
                <div class="action-info">
                  <app-icon name="truck" [size]="24" />
                  <div>
                    <h4>Registrar Entrega</h4>
                    <p>O pedido está pronto para ser entregue</p>
                  </div>
                </div>
                <app-button
                  iconLeft="package-check"
                  (clicked)="registerDelivery(order.id)"
                  [loading]="registeringDelivery()"
                >
                  Confirmar Entrega
                </app-button>
              </div>
            </app-card>
          </section>
        }
      }
    </div>
  `,
  styles: [`
    .order-detail-page {
      max-width: 1000px;
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-16);
    }
    
    .page-header { animation: fadeInDown 0.4s ease-out; }
    
    .back-link {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin-bottom: var(--space-4);
      padding: var(--space-2) var(--space-3);
      border-radius: var(--radius-lg);
      transition: all var(--transition-fast);
      &:hover { color: var(--text-primary); background: var(--bg-secondary); }
    }
    
    .header-info {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      margin-bottom: var(--space-1);
    }
    
    .page-title {
      font-family: var(--font-mono, monospace);
      font-size: var(--text-3xl);
      font-weight: var(--font-bold);
      color: var(--text-primary);
      margin: 0;
    }
    
    .page-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    .status-timeline {
      display: flex;
      justify-content: space-between;
      position: relative;
      padding: var(--space-4) 0;
    }
    
    .timeline-item {
      display: flex;
      flex-direction: column;
      align-items: center;
      flex: 1;
      position: relative;
    }
    
    .timeline-marker {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-tertiary);
      color: var(--text-muted);
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      border: 2px solid var(--border-primary);
      z-index: 1;
      transition: all var(--transition-base);
    }
    
    .timeline-content { margin-top: var(--space-3); text-align: center; }
    .timeline-label { font-size: var(--text-sm); color: var(--text-muted); font-weight: var(--font-medium); }
    
    .timeline-connector {
      position: absolute;
      top: 17px;
      left: 50%;
      width: 100%;
      height: 2px;
      background: var(--border-primary);
      z-index: 0;
    }
    
    .timeline-item.completed .timeline-marker { background: var(--color-success); border-color: var(--color-success); color: white; }
    .timeline-item.completed .timeline-connector { background: var(--color-success); }
    .timeline-item.completed .timeline-label { color: var(--color-success); }
    
    .timeline-item.current .timeline-marker {
      background: var(--color-primary-500);
      border-color: var(--color-primary-500);
      color: white;
      box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.2);
    }
    .timeline-item.current .timeline-label { color: var(--color-primary-400); font-weight: var(--font-semibold); }
    
    .status-actions {
      display: flex;
      justify-content: center;
      gap: var(--space-3);
      margin-top: var(--space-6);
      padding-top: var(--space-4);
      border-top: 1px solid var(--border-primary);
    }
    
    .content-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-6);
    }
    
    .info-grid { display: flex; flex-direction: column; gap: var(--space-4); }
    
    .info-item {
      display: flex;
      gap: var(--space-3);
      padding: var(--space-4);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      color: var(--text-muted);
    }
    
    .info-content { display: flex; flex-direction: column; gap: var(--space-1); }
    .info-label { font-size: var(--text-xs); color: var(--text-muted); text-transform: uppercase; letter-spacing: var(--tracking-wide); }
    .info-value { font-size: var(--text-base); color: var(--text-primary); font-weight: var(--font-medium); }
    .info-value.highlight { color: var(--color-success); font-size: var(--text-lg); font-weight: var(--font-bold); }
    .info-value.success { color: var(--color-success); }
    
    .address-content { display: flex; gap: var(--space-4); }
    
    .address-icon {
      flex-shrink: 0;
      width: 56px;
      height: 56px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, rgba(99, 102, 241, 0.2) 0%, rgba(99, 102, 241, 0.1) 100%);
      border-radius: var(--radius-xl);
      color: var(--color-primary-400);
    }
    
    .address-details { display: flex; flex-direction: column; gap: var(--space-2); }
    .address-street { font-weight: var(--font-semibold); font-size: var(--text-base); color: var(--text-primary); }
    .address-neighborhood, .address-city { font-size: var(--text-sm); color: var(--text-secondary); }
    .address-cep { font-size: var(--text-sm); color: var(--text-muted); font-family: var(--font-mono, monospace); margin-top: var(--space-1); }
    
    .address-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-8);
      color: var(--text-muted);
      text-align: center;
    }
    
    .action-content {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
    }
    
    .action-info {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      color: var(--color-primary-400);
      h4 { margin: 0; font-size: var(--text-lg); color: var(--text-primary); }
      p { margin: 0; font-size: var(--text-sm); color: var(--text-secondary); }
    }
    
    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @media (max-width: 768px) {
      .content-grid { grid-template-columns: 1fr; }
      .status-timeline { flex-direction: column; gap: var(--space-4); }
      .timeline-item { flex-direction: row; gap: var(--space-3); }
      .timeline-content { margin: 0; text-align: left; }
      .timeline-connector { display: none; }
      .action-content { flex-direction: column; text-align: center; }
      .action-info { flex-direction: column; }
    }
  `],
})
export class OrderDetailPageComponent implements OnInit {
  readonly orderService = inject(OrderService);
  readonly deliveryService = inject(DeliveryService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);

  readonly ORDER_STATUS_CONFIG = ORDER_STATUS_CONFIG;
  readonly order = this.orderService.selectedOrder;
  readonly updatingStatus = signal(false);
  readonly registeringDelivery = signal(false);

  readonly statusTimeline = [
    { status: 'Pending' as OrderStatus, label: 'Pendente' },
    { status: 'Confirmed' as OrderStatus, label: 'Confirmado' },
    { status: 'InTransit' as OrderStatus, label: 'Em Trânsito' },
    { status: 'Delivered' as OrderStatus, label: 'Entregue' },
  ];

  ngOnInit(): void {
    const orderId = this.route.snapshot.params['id'];
    if (orderId) {
      this.orderService.loadOrderById(orderId).subscribe();
    }
  }

  getStatus(order: Order): OrderStatus {
    return getStatusFromNumber(order.status);
  }

  isStatusActive(status: OrderStatus, currentStatus: OrderStatus): boolean {
    const order = ['Pending', 'Confirmed', 'InTransit', 'Delivered'];
    return order.indexOf(status) <= order.indexOf(currentStatus);
  }

  isStatusCompleted(status: OrderStatus, currentStatus: OrderStatus): boolean {
    const order = ['Pending', 'Confirmed', 'InTransit', 'Delivered'];
    return order.indexOf(status) < order.indexOf(currentStatus);
  }

  getValidTransitions(status: OrderStatus): OrderStatus[] {
    return STATUS_TRANSITIONS[status] || [];
  }

  updateStatus(orderId: string, newStatus: OrderStatus): void {
    this.updatingStatus.set(true);
    this.orderService.updateOrderStatus(orderId, newStatus).subscribe({
      next: () => {
        this.toastService.success(`Status atualizado para ${ORDER_STATUS_CONFIG[newStatus].label}`);
        this.updatingStatus.set(false);
      },
      error: () => this.updatingStatus.set(false),
    });
  }

  registerDelivery(orderId: string): void {
    this.registeringDelivery.set(true);
    this.deliveryService.registerDelivery(orderId).subscribe({
      next: () => {
        this.toastService.success('Entrega registrada com sucesso!');
        this.orderService.loadOrderById(orderId).subscribe();
        this.registeringDelivery.set(false);
      },
      error: () => this.registeringDelivery.set(false),
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR');
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString('pt-BR');
  }

  formatCep(cep: string): string {
    const digits = cep.replace(/\D/g, '');
    return `${digits.slice(0, 5)}-${digits.slice(5, 8)}`;
  }
}