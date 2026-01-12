import { Component, ChangeDetectionStrategy, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';
import { NotificationService, AuthService, ToastService, OrderService } from '@core/services';
import { ToastContainerComponent } from '../../ui';

@Component({
  selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, SidebarComponent, HeaderComponent, ToastContainerComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="layout">
      <app-sidebar />
      <app-header />
      
      <main class="main-content">
        <router-outlet />
      </main>
      
      <app-toast-container />
    </div>
  `,
  styles: [`
    .layout {
      min-height: 100vh;
      background: var(--bg-primary);
    }
    
    .main-content {
      margin-left: var(--current-sidebar-width, var(--sidebar-width));
      margin-top: var(--header-height);
      padding: var(--space-6);
      min-height: calc(100vh - var(--header-height));
      transition: margin-left var(--transition-base);
    }
    
    @media (max-width: 768px) {
      .main-content {
        margin-left: 0;
        padding: var(--space-4);
      }
    }
  `],
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  private readonly notificationService = inject(NotificationService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly orderService = inject(OrderService);
  
  private subscriptions: Subscription[] = [];

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.notificationService.startConnection();
      this.subscribeToRealTimeEvents();
    }
  }

  ngOnDestroy(): void {
    this.notificationService.stopConnection();
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
  
 private subscribeToRealTimeEvents(): void {
  const notificationSub = this.notificationService.newNotification$.subscribe(notification => {
    this.toastService.info(notification.message, {
      title: this.getNotificationTitle(notification.type),
      duration: 6000,
    });
    this.notificationService.loadNotifications().subscribe();
    this.notificationService.loadUnreadCount().subscribe();
  });
  this.subscriptions.push(notificationSub);
  
  const orderStatusSub = this.notificationService.orderStatusChanged$.subscribe(event => {
    this.orderService.loadOrders({ limit: 100 }).subscribe();
    
    this.notificationService.loadNotifications().subscribe();
    this.notificationService.loadUnreadCount().subscribe();
    
    const statusLabels: Record<string, string> = {
      'Pending': 'Pendente',
      'Confirmed': 'Confirmado', 
      'InTransit': 'Em Trânsito',
      'Delivered': 'Entregue',
      'Cancelled': 'Cancelado',
    };
    
    const newStatusLabel = statusLabels[event.newStatus] || event.newStatus;
    this.toastService.info(`Pedido ${event.orderNumber} → ${newStatusLabel}`, {
      title: 'Status Atualizado',
    });
  });
  this.subscriptions.push(orderStatusSub);
}
  
  private getNotificationTitle(type: string): string {
    const titles: Record<string, string> = {
      'OrderCreated': 'Novo Pedido',
      'OrderStatusChanged': 'Status Atualizado',
      'OrderDelivered': 'Pedido Entregue',
    };
    return titles[type] || 'Notificação';
  }
}