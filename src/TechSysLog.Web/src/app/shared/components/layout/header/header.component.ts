import { Component, ChangeDetectionStrategy, inject, signal, effect } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService, NotificationService } from '@core/services';
import { IconComponent, IconName } from '../../ui';

// ============================================================================
// Header Component
// Top header with notifications and user menu
// ============================================================================

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <header class="header">
      <div class="header-left">
        <button class="mobile-menu-btn" (click)="toggleMobileMenu()">
          <app-icon name="menu" [size]="24" />
        </button>
      </div>
      
      <div class="header-right">
        <!-- Connection Status -->
        <div 
          class="connection-status" 
          [class.connected]="notificationService.isConnected()"
          [title]="notificationService.isConnected() ? 'Conectado' : 'Desconectado'"
        >
          <app-icon 
            [name]="notificationService.isConnected() ? 'wifi' : 'wifi-off'" 
            [size]="16" 
          />
        </div>
        
        <!-- Notifications -->
        <div class="notification-wrapper">
          <button 
            class="notification-btn" 
            (click)="toggleNotifications()"
            [class.has-unread]="notificationService.hasUnread()"
          >
            <app-icon 
              [name]="notificationService.hasUnread() ? 'bell-ring' : 'bell'" 
              [size]="20" 
            />
            @if (notificationService.hasUnread()) {
              <span class="notification-badge">
                {{ notificationService.unreadCount() > 99 ? '99+' : notificationService.unreadCount() }}
              </span>
            }
          </button>
          
          @if (showNotifications()) {
            <div class="notification-dropdown">
              <div class="notification-header">
                <h4>Notificações</h4>
                @if (notificationService.hasUnread()) {
                  <button 
                    class="mark-all-read"
                    (click)="markAllAsRead()"
                  >
                    Marcar todas como lidas
                  </button>
                }
              </div>
              
              <div class="notification-list">
                @if (notificationService.isLoading()) {
                  <div class="notification-loading">
                    <app-icon name="loader" [size]="24" className="spin" />
                  </div>
                } @else if (notificationService.notifications().length === 0) {
                  <div class="notification-empty">
                    <app-icon name="bell" [size]="32" />
                    <p>Nenhuma notificação</p>
                  </div>
                } @else {
                  @for (notification of notificationService.notifications().slice(0, 5); track notification.id) {
                    <div 
                      class="notification-item"
                      [class.unread]="!notification.read"
                      (click)="markAsRead(notification.id)"
                    >
                      <div class="notification-icon">
                        <app-icon [name]="$any(getNotificationIcon(notification.type))" [size]="18" />
                      </div>
                      <div class="notification-content">
                        <p class="notification-message">{{ notification.message }}</p>
                        <span class="notification-time">{{ formatTime(notification.createdAt) }}</span>
                      </div>
                    </div>
                  }
                }
              </div>
              
              <a routerLink="/notifications" class="view-all" (click)="closeNotifications()">
                Ver todas as notificações
              </a>
            </div>
          }
        </div>
        
        <!-- User Menu -->
        <div class="user-wrapper">
          <button class="user-btn" (click)="toggleUserMenu()">
            <div class="user-avatar">
              {{ getUserInitials() }}
            </div>
            @if (authService.user(); as user) {
              <div class="user-info">
                <span class="user-name">{{ user.name }}</span>
                <span class="user-role">{{ getRoleLabel(user.role) }}</span>
              </div>
            }
            <app-icon name="chevron-down" [size]="16" />
          </button>
          
          @if (showUserMenu()) {
            <div class="user-dropdown">
              <button class="dropdown-item" (click)="logout()">
                <app-icon name="log-out" [size]="18" />
                Sair
              </button>
            </div>
          }
        </div>
      </div>
    </header>
    
    @if (showNotifications() || showUserMenu()) {
      <div class="backdrop" (click)="closeAll()"></div>
    }
  `,
  styles: [`
    .header {
      position: fixed;
      top: 0;
      right: 0;
      left: var(--current-sidebar-width, var(--sidebar-width));
      height: var(--header-height);
      background: var(--bg-elevated);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--border-primary);
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0 var(--space-6);
      z-index: var(--z-sticky);
      transition: left var(--transition-base);
    }
    
    .header-left {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    
    .mobile-menu-btn {
      display: none;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background: transparent;
      color: var(--text-secondary);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
    }
    
    .search-wrapper {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-2) var(--space-4);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-lg);
      color: var(--text-muted);
      min-width: 300px;
      
      &:focus-within {
        border-color: var(--color-primary-500);
        box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
      }
    }
    
    .search-input {
      flex: 1;
      background: transparent;
      border: none;
      color: var(--text-primary);
      font-size: var(--text-sm);
      
      &::placeholder {
        color: var(--text-muted);
      }
      
      &:focus {
        outline: none;
      }
    }
    
    .header-right {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }
    
    .connection-status {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
      color: var(--color-error);
      
      &.connected {
        color: var(--color-success);
      }
    }
    
    .notification-wrapper,
    .user-wrapper {
      position: relative;
    }
    
    .notification-btn {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background: transparent;
      color: var(--text-secondary);
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
      
      &.has-unread {
        color: var(--color-primary-400);
      }
    }
    
    .notification-badge {
      position: absolute;
      top: 4px;
      right: 4px;
      min-width: 18px;
      height: 18px;
      padding: 0 4px;
      background: var(--color-error);
      color: white;
      font-size: 10px;
      font-weight: var(--font-bold);
      border-radius: var(--radius-full);
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .notification-dropdown,
    .user-dropdown {
      position: absolute;
      top: calc(100% + var(--space-2));
      right: 0;
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      box-shadow: var(--shadow-xl);
      overflow: hidden;
      animation: fadeInDown 0.2s ease-out;
      z-index: var(--z-dropdown);
    }
    
    .notification-dropdown {
      width: 360px;
    }
    
    .notification-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: var(--space-4);
      border-bottom: 1px solid var(--border-primary);
      
      h4 {
        font-size: var(--text-base);
        font-weight: var(--font-semibold);
        margin: 0;
      }
    }
    
    .mark-all-read {
      font-size: var(--text-xs);
      color: var(--color-primary-400);
      background: none;
      
      &:hover {
        text-decoration: underline;
      }
    }
    
    .notification-list {
      max-height: 320px;
      overflow-y: auto;
    }
    
    .notification-item {
      display: flex;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      cursor: pointer;
      transition: background var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
      }
      
      &.unread {
        background: rgba(99, 102, 241, 0.05);
      }
    }
    
    .notification-icon {
      flex-shrink: 0;
      width: 32px;
      height: 32px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-tertiary);
      border-radius: var(--radius-md);
      color: var(--color-primary-400);
    }
    
    .notification-content {
      flex: 1;
      min-width: 0;
    }
    
    .notification-message {
      font-size: var(--text-sm);
      color: var(--text-primary);
      margin: 0 0 var(--space-1);
      line-height: var(--leading-snug);
    }
    
    .notification-time {
      font-size: var(--text-xs);
      color: var(--text-muted);
    }
    
    .notification-loading,
    .notification-empty {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: var(--space-2);
      padding: var(--space-8);
      color: var(--text-muted);
      
      p {
        font-size: var(--text-sm);
        margin: 0;
      }
    }
    
    .view-all {
      display: block;
      padding: var(--space-3);
      text-align: center;
      font-size: var(--text-sm);
      color: var(--color-primary-400);
      border-top: 1px solid var(--border-primary);
      
      &:hover {
        background: var(--bg-tertiary);
      }
    }
    
    .user-btn {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-2);
      padding-right: var(--space-3);
      background: var(--bg-tertiary);
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--border-primary);
      }
    }
    
    .user-avatar {
      width: 36px;
      height: 36px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, var(--color-primary-500) 0%, var(--color-primary-600) 100%);
      border-radius: var(--radius-md);
      color: white;
      font-size: var(--text-sm);
      font-weight: var(--font-semibold);
    }
    
    .user-info {
      display: flex;
      flex-direction: column;
      text-align: left;
    }
    
    .user-name {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
    }
    
    .user-role {
      font-size: var(--text-xs);
      color: var(--text-muted);
    }
    
    .user-dropdown {
      width: 200px;
      padding: var(--space-2);
    }
    
    .dropdown-item {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      width: 100%;
      padding: var(--space-3);
      border-radius: var(--radius-md);
      font-size: var(--text-sm);
      color: var(--text-secondary);
      text-decoration: none;
      background: none;
      text-align: left;
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
    }
    
    .backdrop {
      position: fixed;
      inset: 0;
      z-index: calc(var(--z-dropdown) - 1);
    }
    
    @keyframes fadeInDown {
      from {
        opacity: 0;
        transform: translateY(-10px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }
    
    @media (max-width: 768px) {
      .header {
        left: 0;
      }
      
      .mobile-menu-btn {
        display: flex;
      }
      
      .search-wrapper {
        display: none;
      }
      
      .user-info {
        display: none;
      }
    }
  `],
})
export class HeaderComponent {
  readonly authService = inject(AuthService);
  readonly notificationService = inject(NotificationService);

  readonly showNotifications = signal(false);
  readonly showUserMenu = signal(false);

  constructor() {
    // Load notifications on init
    effect(() => {
      if (this.authService.isAuthenticated()) {
        this.notificationService.loadNotifications().subscribe();
        this.notificationService.loadUnreadCount().subscribe();
      }
    });
  }

  toggleMobileMenu(): void {
  }

  toggleNotifications(): void {
    this.showNotifications.update(v => !v);
    this.showUserMenu.set(false);
  }

  toggleUserMenu(): void {
    this.showUserMenu.update(v => !v);
    this.showNotifications.set(false);
  }

  closeNotifications(): void {
    this.showNotifications.set(false);
  }

  closeUserMenu(): void {
    this.showUserMenu.set(false);
  }

  closeAll(): void {
    this.showNotifications.set(false);
    this.showUserMenu.set(false);
  }

  markAsRead(id: string): void {
    this.notificationService.markAsRead(id).subscribe();
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe();
  }

  logout(): void {
    this.closeUserMenu();
    this.notificationService.stopConnection();
    this.authService.logout();
  }

  getUserInitials(): string {
    const name = this.authService.user()?.name || '';
    return name
      .split(' ')
      .map(n => n[0])
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

getRoleLabel(role: string | number | undefined): string {
  if (!role) return '';
  
  const labels: Record<string | number, string> = {
    1: 'Cliente',
    2: 'Operador',
    3: 'Administrador',
    'Admin': 'Administrador',
    'Operator': 'Operador',
    'Customer': 'Cliente',
  };
  
  return labels[role] || '';
}

  getNotificationIcon(type: string): IconName {
    const icons: Record<string, IconName> = {
      OrderCreated: 'plus-circle',
      OrderStatusChanged: 'refresh-cw',
      OrderDelivered: 'package-check',
    };
    return icons[type] || 'bell';
  }

  formatTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    
    const minutes = Math.floor(diff / 60000);
    const hours = Math.floor(diff / 3600000);
    const days = Math.floor(diff / 86400000);
    
    if (minutes < 1) return 'Agora';
    if (minutes < 60) return `${minutes}min atrás`;
    if (hours < 24) return `${hours}h atrás`;
    if (days < 7) return `${days}d atrás`;
    
    return date.toLocaleDateString('pt-BR');
  }
}
