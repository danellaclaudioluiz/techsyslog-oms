import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { OrderService, DeliveryService, AuthService, ToastService } from '@core/services';
import { Order, OrderStatus, getStatusFromNumber } from '@core/models';
import { 
  CardComponent, 
  IconComponent, 
  ButtonComponent,
  OrderStatusBadgeComponent,
  LoaderComponent,
  EmptyStateComponent,
} from '@shared/components/ui';

@Component({
  selector: 'app-deliveries-list-page',
  standalone: true,
  imports: [
    RouterLink,
    IconComponent,
    ButtonComponent,
    OrderStatusBadgeComponent,
    LoaderComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="deliveries-page">
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <div class="header-icon">
            <app-icon name="truck" [size]="32" />
          </div>
          <div>
            <h1 class="page-title">Entregas</h1>
            <p class="page-subtitle">Acompanhe os pedidos em trânsito e entregues</p>
          </div>
        </div>
      </header>
      
      <!-- Stats Cards -->
      <section class="stats-section">
        <div class="stat-card in-transit">
          <div class="stat-icon">
            <app-icon name="truck" [size]="24" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ inTransitOrders().length }}</span>
            <span class="stat-label">Em Trânsito</span>
          </div>
        </div>
        
        <div class="stat-card delivered">
          <div class="stat-icon">
            <app-icon name="package-check" [size]="24" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ deliveredOrders().length }}</span>
            <span class="stat-label">Entregues</span>
          </div>
        </div>
        
        <div class="stat-card total">
          <div class="stat-icon">
            <app-icon name="dollar-sign" [size]="24" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ formatCurrency(totalValue()) }}</span>
            <span class="stat-label">Valor Total</span>
          </div>
        </div>
      </section>
      
      <!-- Tabs -->
      <section class="tabs-section">
        <div class="tabs">
          <button 
            class="tab" 
            [class.active]="activeTab() === 'in-transit'"
            (click)="setTab('in-transit')"
          >
            <app-icon name="truck" [size]="18" />
            Em Trânsito
            <span class="tab-count">{{ inTransitOrders().length }}</span>
          </button>
          <button 
            class="tab" 
            [class.active]="activeTab() === 'delivered'"
            (click)="setTab('delivered')"
          >
            <app-icon name="package-check" [size]="18" />
            Entregues
            <span class="tab-count">{{ deliveredOrders().length }}</span>
          </button>
        </div>
      </section>
      
      <!-- Content -->
      <section class="content-section">
        @if (orderService.isLoading()) {
          <div class="loading-state">
            <app-loader size="lg" text="Carregando entregas..." />
          </div>
        } @else if (currentOrders().length === 0) {
          <app-empty-state
            [icon]="activeTab() === 'in-transit' ? 'truck' : 'package-check'"
            [title]="activeTab() === 'in-transit' ? 'Nenhuma entrega em trânsito' : 'Nenhuma entrega realizada'"
            [description]="activeTab() === 'in-transit' 
              ? 'Quando um pedido for despachado, ele aparecerá aqui.' 
              : 'As entregas concluídas aparecerão aqui.'"
          />
        } @else {
          <div class="deliveries-grid">
            @for (order of currentOrders(); track order.id; let i = $index) {
              <div class="delivery-card" [style.--delay]="i * 0.05 + 's'">
                <div class="card-header">
                  <span class="order-number">{{ order.orderNumber }}</span>
                  <app-order-status-badge [status]="getStatus(order)" size="sm" />
                </div>
                
                <div class="card-body">
                  <div class="order-info-row">
                    <app-icon name="package" [size]="16" />
                    <span>{{ order.description }}</span>
                  </div>
                  
                  <div class="order-info-row">
                    <app-icon name="map-pin" [size]="16" />
                    @if (order.deliveryAddress) {
                      <span>{{ order.deliveryAddress.street }}, {{ order.deliveryAddress.number }} - {{ order.deliveryAddress.city }}</span>
                    } @else {
                      <span>Endereço não disponível</span>
                    }
                  </div>
                  
                  <div class="order-info-row value">
                    <app-icon name="dollar-sign" [size]="16" />
                    <span>{{ formatCurrency(order.value) }}</span>
                  </div>
                </div>
                
                <div class="card-footer">
                  @if (getStatus(order) === 'InTransit' && authService.canManageOrders()) {
                    <app-button
                      size="sm"
                      iconLeft="package-check"
                      [loading]="deliveringOrderId() === order.id"
                      (clicked)="confirmDelivery(order)"
                    >
                      Confirmar Entrega
                    </app-button>
                  } @else if (getStatus(order) === 'Delivered' && order.deliveredAt) {
                    <div class="delivered-info">
                      <app-icon name="check-circle" [size]="16" />
                      <span>Entregue em {{ formatDateTime(order.deliveredAt) }}</span>
                    </div>
                  }
                  
                  <app-button
                    variant="ghost"
                    size="sm"
                    iconLeft="eye"
                    [routerLink]="['/orders', order.id]"
                  >
                    Detalhes
                  </app-button>
                </div>
              </div>
            }
          </div>
        }
      </section>
    </div>
  `,
  styles: [`
    .deliveries-page {
      max-width: var(--content-max-width);
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .page-header { animation: fadeInDown 0.4s ease-out; }
    
    .header-content {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    
    .header-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 64px;
      height: 64px;
      background: linear-gradient(135deg, rgba(139, 92, 246, 0.2) 0%, rgba(139, 92, 246, 0.1) 100%);
      border-radius: var(--radius-xl);
      color: var(--color-status-in-transit);
    }
    
    .page-title {
      font-size: var(--text-3xl);
      font-weight: var(--font-bold);
      color: var(--text-primary);
      margin: 0 0 var(--space-1);
    }
    
    .page-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    /* Stats */
    .stats-section {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--space-4);
    }
    
    .stat-card {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-5);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      animation: fadeInUp 0.4s ease-out backwards;
      
      &:nth-child(1) { animation-delay: 0.1s; }
      &:nth-child(2) { animation-delay: 0.15s; }
      &:nth-child(3) { animation-delay: 0.2s; }
    }
    
    .stat-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 48px;
      height: 48px;
      border-radius: var(--radius-lg);
    }
    
    .stat-card.in-transit .stat-icon {
      background: rgba(139, 92, 246, 0.15);
      color: var(--color-status-in-transit);
    }
    
    .stat-card.delivered .stat-icon {
      background: rgba(16, 185, 129, 0.15);
      color: var(--color-success);
    }
    
    .stat-card.total .stat-icon {
      background: rgba(99, 102, 241, 0.15);
      color: var(--color-primary-400);
    }
    
    .stat-info { display: flex; flex-direction: column; }
    .stat-value { font-size: var(--text-2xl); font-weight: var(--font-bold); color: var(--text-primary); }
    .stat-label { font-size: var(--text-sm); color: var(--text-muted); }
    
    /* Tabs */
    .tabs {
      display: flex;
      gap: var(--space-2);
      padding: var(--space-1);
      background: var(--bg-secondary);
      border-radius: var(--radius-xl);
      width: fit-content;
    }
    
    .tab {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-3) var(--space-5);
      background: transparent;
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      transition: all var(--transition-fast);
      
      &:hover { color: var(--text-primary); background: var(--bg-tertiary); }
      &.active { background: var(--color-primary-500); color: white; }
    }
    
    .tab-count {
      padding: var(--space-1) var(--space-2);
      background: rgba(255, 255, 255, 0.2);
      border-radius: var(--radius-full);
      font-size: var(--text-xs);
    }
    
    .tab:not(.active) .tab-count { background: var(--bg-tertiary); }
    
    /* Content */
    .loading-state { display: flex; justify-content: center; padding: var(--space-16); }
    
    .deliveries-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: var(--space-4);
    }
    
    .delivery-card {
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      overflow: hidden;
      animation: fadeInUp 0.4s ease-out backwards;
      animation-delay: var(--delay);
      transition: all var(--transition-fast);
      
      &:hover { border-color: var(--color-primary-500); }
    }
    
    .card-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4);
      border-bottom: 1px solid var(--border-primary);
      background: var(--bg-tertiary);
    }
    
    .order-number {
      font-family: var(--font-mono, monospace);
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
    }
    
    .card-body {
      padding: var(--space-4);
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    
    .order-info-row {
      display: flex;
      align-items: flex-start;
      gap: var(--space-2);
      font-size: var(--text-sm);
      color: var(--text-secondary);
      
      app-icon { flex-shrink: 0; color: var(--text-muted); margin-top: 2px; }
      
      &.value { color: var(--color-success); font-weight: var(--font-medium); }
    }
    
    .card-footer {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      padding: var(--space-4);
      border-top: 1px solid var(--border-primary);
      background: var(--bg-tertiary);
    }
    
    .delivered-info {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      font-size: var(--text-xs);
      color: var(--color-success);
    }
    
    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @media (max-width: 768px) {
      .stats-section { grid-template-columns: 1fr; }
      .deliveries-grid { grid-template-columns: 1fr; }
      .tabs { width: 100%; }
      .tab { flex: 1; justify-content: center; }
    }
  `],
})
export class DeliveriesListPageComponent implements OnInit {
  readonly orderService = inject(OrderService);
  readonly deliveryService = inject(DeliveryService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  readonly activeTab = signal<'in-transit' | 'delivered'>('in-transit');
  readonly deliveringOrderId = signal<string | null>(null);

  readonly inTransitOrders = computed(() => 
    this.orderService.orders().filter(o => getStatusFromNumber(o.status) === 'InTransit')
  );
  
  readonly deliveredOrders = computed(() => 
    this.orderService.orders().filter(o => getStatusFromNumber(o.status) === 'Delivered')
  );
  
  readonly currentOrders = computed(() => 
    this.activeTab() === 'in-transit' ? this.inTransitOrders() : this.deliveredOrders()
  );
  
  readonly totalValue = computed(() => {
    const orders = [...this.inTransitOrders(), ...this.deliveredOrders()];
    return orders.reduce((sum, o) => sum + o.value, 0);
  });

  ngOnInit(): void {
    this.orderService.loadOrders({ limit: 100 }).subscribe();
  }

  setTab(tab: 'in-transit' | 'delivered'): void {
    this.activeTab.set(tab);
  }

  getStatus(order: Order): OrderStatus {
    return getStatusFromNumber(order.status);
  }

  confirmDelivery(order: Order): void {
    this.deliveringOrderId.set(order.id);
    
    this.deliveryService.registerDelivery(order.id).subscribe({
      next: () => {
        this.toastService.success('Pedido entregue com sucesso!');
        this.orderService.loadOrders({ limit: 100 }).subscribe();
        this.deliveringOrderId.set(null);
      },
      error: () => {
        this.toastService.error('Erro ao confirmar entrega');
        this.deliveringOrderId.set(null);
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatDateTime(dateString: string): string {
    return new Date(dateString).toLocaleString('pt-BR', {
      day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit',
    });
  }
}