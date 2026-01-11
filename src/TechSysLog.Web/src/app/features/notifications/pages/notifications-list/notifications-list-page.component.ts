import { Component, ChangeDetectionStrategy, inject, OnInit, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationService, ToastService } from '@core/services';
import { Notification } from '@core/models';
import { 
  CardComponent, 
  IconComponent,
  IconName, 
  ButtonComponent,
  LoaderComponent,
  EmptyStateComponent,
} from '@shared/components/ui';

@Component({
  selector: 'app-notifications-list-page',
  standalone: true,
  imports: [
    RouterLink,
    IconComponent,
    ButtonComponent,
    LoaderComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="notifications-page">
      <!-- Header -->
      <header class="page-header">
        <div class="header-content">
          <div class="header-icon">
            <app-icon name="bell" [size]="32" />
          </div>
          <div>
            <h1 class="page-title">Notificações</h1>
            <p class="page-subtitle">Acompanhe todas as atualizações do sistema</p>
          </div>
        </div>
        
        <div class="header-actions">
          @if (notificationService.hasUnread()) {
            <app-button 
              variant="secondary"
              iconLeft="check"
              (clicked)="markAllAsRead()"
              [loading]="markingAll"
            >
              Marcar todas como lidas
            </app-button>
          }
        </div>
      </header>
      
      <!-- Stats -->
      <section class="stats-section">
        <div class="stat-card">
          <div class="stat-icon total">
            <app-icon name="bell" [size]="20" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ notificationService.notifications().length }}</span>
            <span class="stat-label">Total</span>
          </div>
        </div>
        
        <div class="stat-card">
          <div class="stat-icon unread">
            <app-icon name="bell-ring" [size]="20" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ notificationService.unreadCount() }}</span>
            <span class="stat-label">Não lidas</span>
          </div>
        </div>
        
        <div class="stat-card">
          <div class="stat-icon read">
            <app-icon name="check-circle" [size]="20" />
          </div>
          <div class="stat-info">
            <span class="stat-value">{{ readCount() }}</span>
            <span class="stat-label">Lidas</span>
          </div>
        </div>
      </section>
      
      <!-- Filters -->
      <section class="filters-section">
        <div class="filter-tabs">
          <button 
            class="filter-tab" 
            [class.active]="filter() === 'all'"
            (click)="setFilter('all')"
          >
            Todas
          </button>
          <button 
            class="filter-tab" 
            [class.active]="filter() === 'unread'"
            (click)="setFilter('unread')"
          >
            Não lidas
            @if (notificationService.unreadCount() > 0) {
              <span class="filter-badge">{{ notificationService.unreadCount() }}</span>
            }
          </button>
          <button 
            class="filter-tab" 
            [class.active]="filter() === 'read'"
            (click)="setFilter('read')"
          >
            Lidas
          </button>
        </div>
      </section>
      
      <!-- Notifications List -->
      <section class="notifications-section">
        @if (notificationService.isLoading()) {
          <div class="loading-state">
            <app-loader size="lg" text="Carregando notificações..." />
          </div>
        } @else if (filteredNotifications().length === 0) {
          <app-empty-state
            icon="bell"
            [title]="getEmptyTitle()"
            [description]="getEmptyDescription()"
          >
            @if (filter() !== 'all') {
              <app-button variant="secondary" (clicked)="setFilter('all')">
                Ver todas
              </app-button>
            }
          </app-empty-state>
        } @else {
          <div class="notifications-list">
            @for (notification of filteredNotifications(); track notification.id; let i = $index) {
              <div 
                class="notification-card" 
                [class.unread]="!notification.read"
                [style.--delay]="i * 0.03 + 's'"
                (click)="markAsRead(notification)"
              >
                <div class="notification-icon" [class]="getNotificationTypeClass(notification.type)">
                  <app-icon [name]="getNotificationIcon(notification.type)" [size]="20" />
                </div>
                
                <div class="notification-content">
                  <p class="notification-message">{{ notification.message }}</p>
                  <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
                </div>
                
                <div class="notification-actions">
                  @if (!notification.read) {
                    <div class="unread-indicator" title="Não lida"></div>
                  }
                  
                  @if (getOrderId(notification)) {
                    <a 
                      [routerLink]="['/orders', getOrderId(notification)]" 
                      class="action-btn"
                      title="Ver pedido"
                      (click)="$event.stopPropagation()"
                    >
                      <app-icon name="external-link" [size]="16" />
                    </a>
                  }
                </div>
              </div>
            }
          </div>
        }
      </section>
    </div>
  `,
  styles: [`
    .notifications-page {
      max-width: var(--content-max-width);
      margin: 0 auto;
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
    }
    
    /* Header */
    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-4);
      animation: fadeInDown 0.4s ease-out;
    }
    
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
      background: linear-gradient(135deg, rgba(99, 102, 241, 0.2) 0%, rgba(99, 102, 241, 0.1) 100%);
      border-radius: var(--radius-xl);
      color: var(--color-primary-400);
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
      display: flex;
      gap: var(--space-4);
    }
    
    .stat-card {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-4) var(--space-5);
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
      width: 40px;
      height: 40px;
      border-radius: var(--radius-lg);
      
      &.total {
        background: rgba(99, 102, 241, 0.15);
        color: var(--color-primary-400);
      }
      
      &.unread {
        background: rgba(245, 158, 11, 0.15);
        color: var(--color-warning);
      }
      
      &.read {
        background: rgba(16, 185, 129, 0.15);
        color: var(--color-success);
      }
    }
    
    .stat-info {
      display: flex;
      flex-direction: column;
    }
    
    .stat-value {
      font-size: var(--text-xl);
      font-weight: var(--font-bold);
      color: var(--text-primary);
    }
    
    .stat-label {
      font-size: var(--text-sm);
      color: var(--text-muted);
    }
    
    /* Filters */
    .filters-section {
      animation: fadeInUp 0.4s ease-out 0.25s backwards;
    }
    
    .filter-tabs {
      display: flex;
      gap: var(--space-2);
      padding: var(--space-1);
      background: var(--bg-secondary);
      border-radius: var(--radius-xl);
      width: fit-content;
    }
    
    .filter-tab {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-4);
      background: transparent;
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      transition: all var(--transition-fast);
      
      &:hover {
        color: var(--text-primary);
        background: var(--bg-tertiary);
      }
      
      &.active {
        background: var(--color-primary-500);
        color: white;
      }
    }
    
    .filter-badge {
      padding: 2px var(--space-2);
      background: rgba(255, 255, 255, 0.2);
      border-radius: var(--radius-full);
      font-size: var(--text-xs);
    }
    
    .filter-tab:not(.active) .filter-badge {
      background: var(--color-warning);
      color: white;
    }
    
    /* Notifications List */
    .loading-state {
      display: flex;
      justify-content: center;
      padding: var(--space-16);
    }
    
    .notifications-list {
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
    }
    
    .notification-card {
      display: flex;
      align-items: flex-start;
      gap: var(--space-4);
      padding: var(--space-4);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      cursor: pointer;
      transition: all var(--transition-fast);
      animation: fadeInUp 0.3s ease-out backwards;
      animation-delay: var(--delay);
      
      &:hover {
        border-color: var(--color-primary-500);
        background: var(--bg-tertiary);
      }
      
      &.unread {
        background: rgba(99, 102, 241, 0.05);
        border-left: 3px solid var(--color-primary-500);
      }
    }
    
    .notification-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 44px;
      height: 44px;
      border-radius: var(--radius-lg);
      flex-shrink: 0;
      
      &.order-created {
        background: rgba(16, 185, 129, 0.15);
        color: var(--color-success);
      }
      
      &.order-status-changed {
        background: rgba(99, 102, 241, 0.15);
        color: var(--color-primary-400);
      }
      
      &.order-delivered {
        background: rgba(139, 92, 246, 0.15);
        color: var(--color-status-in-transit);
      }
      
      &.default {
        background: var(--bg-tertiary);
        color: var(--text-muted);
      }
    }
    
    .notification-content {
      flex: 1;
      min-width: 0;
    }
    
    .notification-message {
      font-size: var(--text-base);
      color: var(--text-primary);
      margin: 0 0 var(--space-1);
      line-height: var(--leading-relaxed);
    }
    
    .notification-time {
      font-size: var(--text-sm);
      color: var(--text-muted);
    }
    
    .notification-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      flex-shrink: 0;
    }
    
    .unread-indicator {
      width: 10px;
      height: 10px;
      background: var(--color-primary-500);
      border-radius: 50%;
      animation: pulse 2s infinite;
    }
    
    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }
    
    .action-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      color: var(--text-muted);
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-secondary);
        color: var(--color-primary-400);
      }
    }
    
    /* Animations */
    @keyframes fadeInDown {
      from { opacity: 0; transform: translateY(-20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    @keyframes fadeInUp {
      from { opacity: 0; transform: translateY(20px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    /* Responsive */
    @media (max-width: 768px) {
      .page-header {
        flex-direction: column;
        align-items: stretch;
      }
      
      .header-actions {
        align-self: flex-start;
      }
      
      .stats-section {
        flex-direction: column;
      }
      
      .filter-tabs {
        width: 100%;
      }
      
      .filter-tab {
        flex: 1;
        justify-content: center;
      }
    }
  `],
})
export class NotificationsListPageComponent implements OnInit {
  readonly notificationService = inject(NotificationService);
  private readonly toastService = inject(ToastService);

  filter = signal<'all' | 'unread' | 'read'>('all');
  markingAll = false;

  readonly readCount = computed(() => {
    const total = this.notificationService.notifications().length;
    const unread = this.notificationService.unreadCount();
    return total - unread;
  });

  readonly filteredNotifications = computed(() => {
    const notifications = this.notificationService.notifications();
    const currentFilter = this.filter();
    
    switch (currentFilter) {
      case 'unread':
        return notifications.filter(n => !n.read);
      case 'read':
        return notifications.filter(n => n.read);
      default:
        return notifications;
    }
  });

  ngOnInit(): void {
    this.notificationService.loadNotifications().subscribe();
    this.notificationService.loadUnreadCount().subscribe();
  }

  setFilter(filter: 'all' | 'unread' | 'read'): void {
    this.filter.set(filter);
  }

  markAsRead(notification: Notification): void {
    if (!notification.read) {
      this.notificationService.markAsRead(notification.id).subscribe();
    }
  }

  markAllAsRead(): void {
    this.markingAll = true;
    this.notificationService.markAllAsRead().subscribe({
      next: () => {
        this.toastService.success('Todas as notificações foram marcadas como lidas');
        this.markingAll = false;
      },
      error: () => {
        this.toastService.error('Erro ao marcar notificações');
        this.markingAll = false;
      },
    });
  }

  getNotificationIcon(type: string): IconName {
    const icons: Record<string, IconName> = {
      'OrderCreated': 'plus-circle',
      'OrderStatusChanged': 'refresh-cw',
      'OrderDelivered': 'package-check',
    };
    return icons[type] || 'bell';
  }

  getNotificationTypeClass(type: string): string {
    const classes: Record<string, string> = {
      'OrderCreated': 'order-created',
      'OrderStatusChanged': 'order-status-changed',
      'OrderDelivered': 'order-delivered',
    };
    return classes[type] || 'default';
  }

  getOrderId(notification: Notification): string | null {
    if (notification.data) {
      try {
        const data = typeof notification.data === 'string' 
          ? JSON.parse(notification.data) 
          : notification.data;
        return data.OrderId || data.orderId || null;
      } catch {
        return null;
      }
    }
    return null;
  }

  getEmptyTitle(): string {
    switch (this.filter()) {
      case 'unread': return 'Nenhuma notificação não lida';
      case 'read': return 'Nenhuma notificação lida';
      default: return 'Nenhuma notificação';
    }
  }

  getEmptyDescription(): string {
    switch (this.filter()) {
      case 'unread': return 'Você está em dia com todas as notificações!';
      case 'read': return 'As notificações lidas aparecerão aqui.';
      default: return 'Quando houver atualizações, elas aparecerão aqui.';
    }
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
    if (days < 7) return `${days} dias atrás`;
    
    return date.toLocaleDateString('pt-BR', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }
}

// Importar signal
import { signal } from '@angular/core';