import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { OrderService, AuthService, NotificationService } from '@core/services';
import { Order, OrderStatus, ORDER_STATUS_CONFIG, getStatusFromNumber } from '@core/models';
import { 
  CardComponent, 
  IconComponent,
  IconName, 
  ButtonComponent,
  OrderStatusBadgeComponent,
  LoaderComponent,
  EmptyStateComponent 
} from '@shared/components/ui';

interface StatCard {
  readonly label: string;
  readonly value: number;
  readonly icon: IconName;
  readonly color: string;
  readonly bgColor: string;
  readonly trend?: number;
}

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [
    RouterLink,
    CardComponent,
    IconComponent,
    ButtonComponent,
    OrderStatusBadgeComponent,
    LoaderComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="dashboard-page">
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <h1 class="page-title">Dashboard</h1>
          <p class="page-subtitle">
            Bem-vindo, {{ (authService.user()?.name?.split(' ') || ['usuário'])[0] }}!
            Aqui está um resumo dos seus pedidos.
          </p>
        </div>
        
        <div class="header-actions">
          @if (authService.canManageOrders()) {
            <app-button routerLink="/orders/new" iconLeft="plus">
              Novo Pedido
            </app-button>
          }
        </div>
      </header>
      
      <!-- Stats Grid -->
      <section class="stats-section">
        <div class="stats-grid">
          @for (stat of statCards(); track stat.label; let i = $index) {
            <div class="stat-card" [style.--delay]="i * 0.1 + 's'">
              <div class="stat-icon" [style.background]="stat.bgColor" [style.color]="stat.color">
                <app-icon [name]="$any(stat.icon)" [size]="24" />
              </div>
              <div class="stat-content">
                <span class="stat-value">{{ stat.value }}</span>
                <span class="stat-label">{{ stat.label }}</span>
              </div>
              @if (stat.trend !== undefined) {
                <div class="stat-trend" [class.positive]="stat.trend >= 0" [class.negative]="stat.trend < 0">
                  <app-icon [name]="stat.trend >= 0 ? 'trending-up' : 'trending-down'" [size]="16" />
                  {{ stat.trend > 0 ? '+' : '' }}{{ stat.trend }}%
                </div>
              }
            </div>
          }
        </div>
      </section>
      
      <!-- Content Grid -->
      <div class="content-grid">
        <!-- Recent Orders -->
        <section class="recent-orders">
          <app-card title="Pedidos Recentes" subtitle="Últimos pedidos cadastrados">
            <a routerLink="/orders" card-actions class="view-all-link">
              Ver todos
              <app-icon name="arrow-right" [size]="16" />
            </a>
            
            @if (orderService.isLoading()) {
              <div class="loading-state">
                <app-loader size="md" text="Carregando pedidos..." />
              </div>
            } @else if (recentOrders().length === 0) {
              <app-empty-state
                icon="package"
                title="Nenhum pedido"
                description="Comece criando seu primeiro pedido."
              >
                @if (authService.canManageOrders()) {
                  <app-button routerLink="/orders/new" iconLeft="plus" size="sm">
                    Novo Pedido
                  </app-button>
                }
              </app-empty-state>
            } @else {
              <div class="orders-list">
                @for (order of recentOrders(); track order.id; let i = $index) {
                  <a [routerLink]="['/orders', order.id]" class="order-item" [style.--delay]="i * 0.05 + 's'">
                    <div class="order-info">
                      <span class="order-number">{{ order.orderNumber }}</span>
                      <span class="order-description">{{ order.description }}</span>
                    </div>
                    <div class="order-meta">
                      <app-order-status-badge [status]="order.status" size="sm" />
                      <span class="order-value">{{ formatCurrency(order.value) }}</span>
                    </div>
                  </a>
                }
              </div>
            }
          </app-card>
        </section>
        
        <!-- Status Distribution -->
        <section class="status-distribution">
          <app-card title="Distribuição por Status" subtitle="Visão geral dos pedidos">
            <div class="status-chart">
              @for (status of statusDistribution(); track status.status) {
                <div class="status-bar-item">
                  <div class="status-bar-header">
                    <div class="status-info">
                      <span class="status-dot" [style.background]="status.color"></span>
                      <span class="status-name">{{ status.label }}</span>
                    </div>
                    <span class="status-count">{{ status.count }}</span>
                  </div>
                  <div class="status-bar-track">
                    <div 
                      class="status-bar-fill"
                      [style.width.%]="status.percentage"
                      [style.background]="status.color"
                    ></div>
                  </div>
                </div>
              }
            </div>
          </app-card>
          
          <!-- Quick Actions -->
          <app-card title="Ações Rápidas" class="quick-actions-card">
            <div class="quick-actions">
              @if (authService.canManageOrders()) {
                <a routerLink="/orders/new" class="quick-action">
                  <div class="action-icon">
                    <app-icon name="plus-circle" [size]="24" />
                  </div>
                  <span>Novo Pedido</span>
                </a>
              }
              
              <a routerLink="/orders" [queryParams]="{status: 'InTransit'}" class="quick-action">
                <div class="action-icon">
                  <app-icon name="truck" [size]="24" />
                </div>
                <span>Em Trânsito</span>
              </a>
              
              <a routerLink="/orders" [queryParams]="{status: 'Pending'}" class="quick-action">
                <div class="action-icon">
                  <app-icon name="clock" [size]="24" />
                </div>
                <span>Pendentes</span>
              </a>
              
              <a routerLink="/deliveries" class="quick-action">
                <div class="action-icon">
                  <app-icon name="package-check" [size]="24" />
                </div>
                <span>Entregas</span>
              </a>
            </div>
          </app-card>
        </section>
      </div>
      
      <!-- Activity Timeline -->
      <section class="activity-section">
        <app-card title="Atividade Recente" subtitle="Últimas atualizações do sistema">
          @if (notificationService.notifications().length === 0) {
            <div class="no-activity">
              <app-icon name="activity" [size]="32" />
              <p>Nenhuma atividade recente</p>
            </div>
          } @else {
            <div class="activity-timeline">
              @for (notification of notificationService.notifications().slice(0, 5); track notification.id) {
                <div class="activity-item">
                  <div class="activity-icon">
                    <app-icon [name]="$any(getActivityIcon(notification.type))" [size]="16" />
                  </div>
                  <div class="activity-content">
                    <p class="activity-message">{{ notification.message }}</p>
                    <span class="activity-time">{{ formatTime(notification.createdAt) }}</span>
                  </div>
                </div>
              }
            </div>
          }
        </app-card>
      </section>
    </div>
  `,
  styles: [`
    .dashboard-page {
      max-width: var(--content-max-width);
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-4);
      animation: fadeInDown 0.4s ease-out;
    }
    
    .page-title {
      font-family: var(--font-serif);
      font-size: var(--text-4xl);
      font-weight: var(--font-normal);
      color: var(--text-primary);
      margin: 0 0 var(--space-1);
    }
    
    .page-subtitle {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0;
    }
    
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
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
      animation-delay: var(--delay);
      transition: all var(--transition-base);
      
      &:hover {
        border-color: var(--color-neutral-600);
        transform: translateY(-2px);
        box-shadow: var(--shadow-lg);
      }
    }
    
    .stat-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 56px;
      height: 56px;
      border-radius: var(--radius-xl);
      flex-shrink: 0;
    }
    
    .stat-content {
      flex: 1;
      display: flex;
      flex-direction: column;
    }
    
    .stat-value {
      font-size: var(--text-3xl);
      font-weight: var(--font-bold);
      color: var(--text-primary);
      line-height: 1;
    }
    
    .stat-label {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin-top: var(--space-1);
    }
    
    .stat-trend {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      padding: var(--space-1) var(--space-2);
      border-radius: var(--radius-full);
      
      &.positive {
        color: var(--color-success);
        background: rgba(16, 185, 129, 0.1);
      }
      
      &.negative {
        color: var(--color-error);
        background: rgba(239, 68, 68, 0.1);
      }
    }
    
    .content-grid {
      display: grid;
      grid-template-columns: 2fr 1fr;
      gap: var(--space-6);
    }
    
    .recent-orders,
    .status-distribution {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-8);
    }
    
    .view-all-link {
      display: flex;
      align-items: center;
      gap: var(--space-1);
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--color-primary-400);
      
      &:hover {
        gap: var(--space-2);
      }
    }
    
    .orders-list {
      display: flex;
      flex-direction: column;
    }
    
    .order-item {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      padding: var(--space-4);
      border-bottom: 1px solid var(--border-secondary);
      transition: all var(--transition-fast);
      animation: fadeInUp 0.3s ease-out backwards;
      animation-delay: var(--delay);
      
      &:last-child { border-bottom: none; }
      &:hover { background: var(--bg-tertiary); }
    }
    
    .order-info {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    
    .order-number {
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
      font-family: var(--font-mono, monospace);
    }
    
    .order-description {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      max-width: 300px;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    
    .order-meta {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    
    .order-value {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
    }
    
    .status-chart {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
    }
    
    .status-bar-item {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .status-bar-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    
    .status-info {
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }
    
    .status-dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
    }
    
    .status-name {
      font-size: var(--text-sm);
      color: var(--text-secondary);
    }
    
    .status-count {
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
    }
    
    .status-bar-track {
      height: 6px;
      background: var(--bg-tertiary);
      border-radius: var(--radius-full);
      overflow: hidden;
    }
    
    .status-bar-fill {
      height: 100%;
      border-radius: var(--radius-full);
      transition: width 0.6s ease-out;
    }
    
    .quick-actions {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--space-3);
    }
    
    .quick-action {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-4);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      text-decoration: none;
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--border-primary);
        transform: translateY(-2px);
      }
    }
    
    .action-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 44px;
      height: 44px;
      background: var(--bg-secondary);
      border-radius: var(--radius-lg);
      color: var(--color-primary-400);
    }
    
    .quick-action span {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-secondary);
    }
    
    .activity-timeline {
      display: flex;
      flex-direction: column;
    }
    
    .activity-item {
      display: flex;
      gap: var(--space-3);
      padding: var(--space-3) 0;
      border-bottom: 1px solid var(--border-secondary);
      
      &:last-child { border-bottom: none; }
    }
    
    .activity-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      background: var(--bg-tertiary);
      border-radius: var(--radius-md);
      color: var(--color-primary-400);
      flex-shrink: 0;
    }
    
    .activity-content { flex: 1; }
    
    .activity-message {
      font-size: var(--text-sm);
      color: var(--text-primary);
      margin: 0 0 var(--space-1);
    }
    
    .activity-time {
      font-size: var(--text-xs);
      color: var(--text-muted);
    }
    
    .no-activity {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-8);
      color: var(--text-muted);
      
      p { margin: 0; font-size: var(--text-sm); }
    }
    
    @media (max-width: 1024px) {
      .content-grid { grid-template-columns: 1fr; }
      .status-distribution { flex-direction: row; }
      .status-distribution > * { flex: 1; }
    }
    
    @media (max-width: 768px) {
      .page-header { flex-direction: column; }
      .stats-grid { grid-template-columns: repeat(2, 1fr); }
      .status-distribution { flex-direction: column; }
    }
    
    @media (max-width: 480px) {
      .stats-grid { grid-template-columns: 1fr; }
    }
  `],
})
export class DashboardPageComponent implements OnInit {
  readonly authService = inject(AuthService);
  readonly orderService = inject(OrderService);
  readonly notificationService = inject(NotificationService);

  readonly recentOrders = computed(() => this.orderService.orders().slice(0, 5));

  readonly statCards = computed<StatCard[]>(() => {
    const stats = this.orderService.orderStats();
    return [
      {
        label: 'Total de Pedidos',
        value: stats.total,
        icon: 'package',
        color: 'var(--color-primary-400)',
        bgColor: 'rgba(99, 102, 241, 0.15)',
      },
      {
        label: 'Pendentes',
        value: stats.pending,
        icon: 'clock',
        color: 'var(--color-status-pending)',
        bgColor: 'rgba(245, 158, 11, 0.15)',
      },
      {
        label: 'Em Trânsito',
        value: stats.inTransit,
        icon: 'truck',
        color: 'var(--color-status-in-transit)',
        bgColor: 'rgba(139, 92, 246, 0.15)',
      },
      {
        label: 'Entregues',
        value: stats.delivered,
        icon: 'package-check',
        color: 'var(--color-status-delivered)',
        bgColor: 'rgba(16, 185, 129, 0.15)',
      },
    ];
  });

  readonly statusDistribution = computed(() => {
    const stats = this.orderService.orderStats();
    const total = stats.total || 1;

    const statuses: OrderStatus[] = ['Pending', 'Confirmed', 'InTransit', 'Delivered', 'Cancelled'];
    
    return statuses.map(status => {
      const config = ORDER_STATUS_CONFIG[status];
      const key = status.charAt(0).toLowerCase() + status.slice(1).replace(/([A-Z])/g, c => c.toLowerCase());
      const count = (stats as any)[key] || 0;
      
      return {
        status,
        label: config.label,
        color: config.color,
        count,
        percentage: (count / total) * 100,
      };
    });
  });

  ngOnInit(): void {
    this.orderService.loadOrders({}).subscribe();
    this.notificationService.loadNotifications().subscribe();
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);
    
    if (minutes < 1) return 'Agora';
    if (minutes < 60) return `${minutes} min atrás`;
    if (hours < 24) return `${hours}h atrás`;
    if (days < 7) return `${days}d atrás`;
    
    return date.toLocaleDateString('pt-BR');
  }

  getActivityIcon(type: string): IconName {
    const icons: Record<string, IconName> = {
      OrderCreated: 'plus-circle',
      OrderStatusChanged: 'refresh-cw',
      OrderDelivered: 'package-check',
    };
    return icons[type] || 'activity';
  }
}