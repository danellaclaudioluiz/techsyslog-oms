import { Component, ChangeDetectionStrategy, inject, OnInit, signal, computed } from '@angular/core';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { OrderService, AuthService, ToastService } from '@core/services';
import { Order, OrderStatus, ORDER_STATUS_CONFIG, getStatusFromNumber } from '@core/models';
import { 
  CardComponent, 
  IconComponent, 
  ButtonComponent,
  OrderStatusBadgeComponent,
  LoaderComponent,
  EmptyStateComponent,
  InputComponent,
} from '@shared/components/ui';

// ============================================================================
// Orders List Page Component
// List and filter orders
// ============================================================================

@Component({
  selector: 'app-orders-list-page',
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
    <div class="orders-page">
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <h1 class="page-title">Pedidos</h1>
          <p class="page-subtitle">Gerencie todos os pedidos do sistema</p>
        </div>
        
        <div class="header-actions">
          <app-button 
            routerLink="/orders/new" 
            iconLeft="plus"
          >
            Novo Pedido
          </app-button>
        </div>
      </header>
      
      <!-- Filters -->
      <section class="filters-section">
        <div class="search-box">
          <app-icon name="search" [size]="18" />
          <input 
            type="search" 
            placeholder="Buscar por número ou descrição..."
            [value]="searchQuery()"
            (input)="onSearch($event)"
          />
        </div>
        
        <div class="status-filters">
          <button 
            class="filter-chip" 
            [class.active]="!selectedStatus()"
            (click)="filterByStatus(undefined)"
          >
            Todos
            <span class="chip-count">{{ orderService.orderStats().total }}</span>
          </button>
          
          @for (status of statusOptions; track status.value) {
            <button 
              class="filter-chip"
              [class.active]="selectedStatus() === status.value"
              [style.--chip-color]="status.color"
              (click)="filterByStatus(status.value)"
            >
              {{ status.label }}
              <span class="chip-count">{{ getStatusCount(status.value) }}</span>
            </button>
          }
        </div>
      </section>
      
      <!-- Orders List -->
      <section class="orders-section">
        @if (orderService.isLoading()) {
          <div class="loading-state">
            <app-loader size="lg" text="Carregando pedidos..." />
          </div>
        } @else if (filteredOrders().length === 0) {
          <app-empty-state
            icon="package"
            [title]="searchQuery() || selectedStatus() ? 'Nenhum pedido encontrado' : 'Nenhum pedido'"
            [description]="searchQuery() || selectedStatus() 
              ? 'Tente ajustar os filtros de busca.' 
              : 'Comece criando seu primeiro pedido.'"
          >
            @if (!searchQuery() && !selectedStatus()) {
              <app-button routerLink="/orders/new" iconLeft="plus">
                Criar Pedido
              </app-button>
            } @else {
              <app-button (clicked)="clearFilters()" variant="secondary">
                Limpar Filtros
              </app-button>
            }
          </app-empty-state>
        } @else {
          <div class="orders-table-wrapper">
            <table class="orders-table">
              <thead>
                <tr>
                  <th>Número</th>
                  <th>Descrição</th>
                  <th>Valor</th>
                  <th>Status</th>
                  <th>Data</th>
                  <th>Ações</th>
                </tr>
              </thead>
              <tbody>
                @for (order of filteredOrders(); track order.id; let i = $index) {
                  <tr 
                    class="order-row" 
                    [style.--delay]="i * 0.03 + 's'"
                    (click)="viewOrder(order)"
                  >
                    <td class="order-number">{{ order.orderNumber }}</td>
                    <td class="order-description">
                      <span>{{ order.description }}</span>
                    </td>
                    <td class="order-value">{{ formatCurrency(order.value) }}</td>
                    <td>
                      <app-order-status-badge [status]="getStatus(order)" size="sm" />
                    </td>
                    <td class="order-date">{{ formatDate(order.createdAt) }}</td>
                    <td class="order-actions" (click)="$event.stopPropagation()">
                      <div class="action-buttons">
                        <button 
                          class="action-btn" 
                          title="Ver detalhes"
                          (click)="viewOrder(order)"
                        >
                          <app-icon name="eye" [size]="16" />
                        </button>
                        
                        @if (canUpdateStatus(order)) {
                          <button 
                            class="action-btn"
                            title="Avançar status"
                            (click)="advanceStatus(order)"
                          >
                            <app-icon name="arrow-right" [size]="16" />
                          </button>
                        }
                        
                        @if (getStatus(order) === 'InTransit') {
                          <button 
                            class="action-btn success"
                            title="Registrar entrega"
                            (click)="registerDelivery(order)"
                          >
                            <app-icon name="package-check" [size]="16" />
                          </button>
                        }
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          </div>
          
          @if (orderService.hasMore()) {
            <div class="load-more">
              <app-button 
                variant="secondary" 
                (clicked)="loadMore()"
                [loading]="loadingMore()"
              >
                Carregar mais
              </app-button>
            </div>
          }
        }
      </section>
    </div>
  `,
  styles: [`
    .orders-page {
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
    
    /* Filters */
    .filters-section {
      display: flex;
      flex-direction: column;
      gap: var(--space-4);
      padding: var(--space-4);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
    }
    
    .search-box {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-3) var(--space-4);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      color: var(--text-muted);
      
      input {
        flex: 1;
        background: transparent;
        border: none;
        color: var(--text-primary);
        font-size: var(--text-base);
        
        &::placeholder {
          color: var(--text-muted);
        }
        
        &:focus {
          outline: none;
        }
      }
    }
    
    .status-filters {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-2);
    }
    
    .filter-chip {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-3);
      background: var(--bg-tertiary);
      border: 1px solid transparent;
      border-radius: var(--radius-full);
      color: var(--text-secondary);
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      cursor: pointer;
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--border-primary);
      }
      
      &.active {
        background: var(--chip-color, var(--color-primary-500));
        color: white;
        border-color: transparent;
      }
    }
    
    .chip-count {
      padding: 0 var(--space-2);
      background: rgba(255, 255, 255, 0.2);
      border-radius: var(--radius-full);
      font-size: var(--text-xs);
    }
    
    .filter-chip:not(.active) .chip-count {
      background: var(--bg-secondary);
    }
    
    /* Loading */
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-16);
    }
    
    /* Table */
    .orders-table-wrapper {
      overflow-x: auto;
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
    }
    
    .orders-table {
      width: 100%;
      border-collapse: collapse;
      
      th, td {
        padding: var(--space-4);
        text-align: left;
      }
      
      th {
        font-size: var(--text-xs);
        font-weight: var(--font-semibold);
        color: var(--text-muted);
        text-transform: uppercase;
        letter-spacing: var(--tracking-wide);
        background: var(--bg-tertiary);
        border-bottom: 1px solid var(--border-primary);
      }
    }
    
    .order-row {
      cursor: pointer;
      transition: background var(--transition-fast);
      animation: fadeIn 0.3s ease-out backwards;
      animation-delay: var(--delay);
      
      &:hover {
        background: var(--bg-tertiary);
      }
      
      &:not(:last-child) {
        border-bottom: 1px solid var(--border-secondary);
      }
    }
    
    .order-number {
      font-family: var(--font-mono, monospace);
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
    }
    
    .order-description {
      max-width: 300px;
      
      span {
        display: block;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
        color: var(--text-secondary);
        font-size: var(--text-sm);
      }
    }
    
    .order-value {
      font-weight: var(--font-medium);
      color: var(--text-primary);
    }
    
    .order-date {
      font-size: var(--text-sm);
      color: var(--text-muted);
    }
    
    .action-buttons {
      display: flex;
      gap: var(--space-1);
    }
    
    .action-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      background: transparent;
      color: var(--text-muted);
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
      
      &.success:hover {
        background: rgba(16, 185, 129, 0.15);
        color: var(--color-success);
      }
    }
    
    .load-more {
      display: flex;
      justify-content: center;
      padding: var(--space-4);
    }
    
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
      }
      
      .orders-table th:nth-child(2),
      .orders-table td:nth-child(2) {
        display: none;
      }
    }
  `],
})
export class OrdersListPageComponent implements OnInit {
  readonly orderService = inject(OrderService);
  readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);

  readonly searchQuery = signal('');
  readonly selectedStatus = signal<OrderStatus | undefined>(undefined);
  readonly loadingMore = signal(false);

  readonly statusOptions = [
    { value: 'Pending' as OrderStatus, label: 'Pendente', color: 'var(--color-status-pending)' },
    { value: 'Confirmed' as OrderStatus, label: 'Confirmado', color: 'var(--color-status-confirmed)' },
    { value: 'InTransit' as OrderStatus, label: 'Em Trânsito', color: 'var(--color-status-in-transit)' },
    { value: 'Delivered' as OrderStatus, label: 'Entregue', color: 'var(--color-status-delivered)' },
    { value: 'Cancelled' as OrderStatus, label: 'Cancelado', color: 'var(--color-status-cancelled)' },
  ];

  readonly filteredOrders = computed(() => {
    let orders = this.orderService.orders();
    const query = this.searchQuery().toLowerCase();
    const status = this.selectedStatus();

    if (query) {
      orders = orders.filter(o =>
        o.orderNumber.toLowerCase().includes(query) ||
        o.description.toLowerCase().includes(query)
      );
    }

    if (status) {
      orders = orders.filter(o => getStatusFromNumber(o.status) === status);
    }

    return orders;
  });

  ngOnInit(): void {
    const statusParam = this.route.snapshot.queryParams['status'];
    if (statusParam) {
      this.selectedStatus.set(statusParam as OrderStatus);
    }

    this.orderService.loadOrders({ limit: 100 }).subscribe();
  }

  onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQuery.set(value);
  }

  filterByStatus(status: OrderStatus | undefined): void {
    this.selectedStatus.set(status);
  }

  clearFilters(): void {
    this.searchQuery.set('');
    this.selectedStatus.set(undefined);
  }

  getStatusCount(status: OrderStatus): number {
    return this.orderService.orders().filter(o => getStatusFromNumber(o.status) === status).length;
  }

  getStatus(order: Order): OrderStatus {
    return getStatusFromNumber(order.status);
  }

  viewOrder(order: Order): void {
    this.router.navigate(['/orders', order.id]);
  }

  canUpdateStatus(order: Order): boolean {
    if (!this.authService.canManageOrders()) return false;
    const validTransitions = this.orderService.getValidTransitions(this.getStatus(order));
    return validTransitions.length > 0;
  }

  advanceStatus(order: Order): void {
    const nextStatus = this.getNextStatus(this.getStatus(order));
    if (nextStatus) {
      this.orderService.updateOrderStatus(order.id, nextStatus).subscribe({
        next: () => {
          this.toastService.success(`Status atualizado para ${ORDER_STATUS_CONFIG[nextStatus].label}`);
        },
      });
    }
  }

  getNextStatus(currentStatus: OrderStatus): OrderStatus | null {
    const transitions: Record<OrderStatus, OrderStatus | null> = {
      Pending: 'Confirmed',
      Confirmed: 'InTransit',
      InTransit: 'Delivered',
      Delivered: null,
      Cancelled: null,
    };
    return transitions[currentStatus];
  }

  registerDelivery(order: Order): void {
    this.router.navigate(['/deliveries/register'], { 
      queryParams: { orderId: order.id } 
    });
  }

  loadMore(): void {
    const pagination = this.orderService.pagination();
    if (!pagination?.cursor) return;

    this.loadingMore.set(true);
    this.orderService.loadOrders({ 
      limit: 20, 
      cursor: pagination.cursor 
    }).subscribe({
      complete: () => this.loadingMore.set(false),
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }
}
