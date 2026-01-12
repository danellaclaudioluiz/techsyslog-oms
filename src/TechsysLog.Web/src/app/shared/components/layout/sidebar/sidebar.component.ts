import { Component, ChangeDetectionStrategy, inject, signal, computed, effect } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '@core/services';
import { IconComponent, IconName } from '../../ui';

// ============================================================================
// Sidebar Component
// Main navigation sidebar
// ============================================================================

interface NavItem {
  readonly label: string;
  readonly icon: IconName;
  readonly route: string;
  readonly roles?: string[];
}

const NAV_ITEMS: NavItem[] = [
  { label: 'Dashboard', icon: 'bar-chart', route: '/dashboard' },
  { label: 'Pedidos', icon: 'package', route: '/orders' },
  { label: 'Entregas', icon: 'truck', route: '/deliveries' },
];

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <aside class="sidebar" [class.collapsed]="collapsed()">
      <!-- Logo -->
      <div class="sidebar-header">
        <a routerLink="/dashboard" class="logo">
          <div class="logo-icon">
            <app-icon name="package" [size]="24" />
          </div>
          @if (!collapsed()) {
            <span class="logo-text">
              <span class="logo-primary">Techsys</span>
              <span class="logo-secondary">Log</span>
            </span>
          }
        </a>
      </div>

      <!-- Navigation -->
      <nav class="sidebar-nav">
        <ul class="nav-list">
          @for (item of visibleNavItems(); track item.route) {
            <li class="nav-item">
              <a
                [routerLink]="item.route"
                routerLinkActive="active"
                [routerLinkActiveOptions]="{ exact: item.route === '/dashboard' }"
                class="nav-link"
                [title]="collapsed() ? item.label : ''"
              >
                <app-icon [name]="$any(item.icon)" [size]="20" />
                @if (!collapsed()) {
                  <span class="nav-label">{{ item.label }}</span>
                }
              </a>
            </li>
          }
        </ul>
      </nav>

      <!-- Footer -->
      <div class="sidebar-footer">
        <button 
          class="collapse-btn" 
          (click)="toggleCollapse()"
          [title]="collapsed() ? 'Expandir' : 'Recolher'"
        >
          <app-icon 
            [name]="$any(collapsed() ? 'chevron-right' : 'chevron-left')" 
            [size]="18" 
          />
        </button>
      </div>
    </aside>
  `,
  styles: [`
    .sidebar {
      position: fixed;
      top: 0;
      left: 0;
      height: 100vh;
      width: var(--sidebar-width);
      background: var(--bg-secondary);
      border-right: 1px solid var(--border-primary);
      display: flex;
      flex-direction: column;
      z-index: var(--z-sticky);
      transition: width var(--transition-base);
    }
    
    .sidebar.collapsed {
      width: var(--sidebar-collapsed-width);
    }
    
    .sidebar-header {
      padding: var(--space-5);
      border-bottom: 1px solid var(--border-primary);
    }
    
    .logo {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      text-decoration: none;
    }
    
    .logo-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      background: linear-gradient(135deg, var(--color-primary-500) 0%, var(--color-primary-600) 100%);
      border-radius: var(--radius-lg);
      color: white;
      flex-shrink: 0;
    }
    
    .logo-text {
      font-size: var(--text-xl);
      font-weight: var(--font-bold);
      letter-spacing: var(--tracking-tight);
    }
    
    .logo-primary {
      color: var(--text-primary);
    }
    
    .logo-secondary {
      color: var(--color-primary-400);
    }
    
    .sidebar-nav {
      flex: 1;
      padding: var(--space-4);
      overflow-y: auto;
    }
    
    .nav-list {
      list-style: none;
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }
    
    .nav-link {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      border-radius: var(--radius-lg);
      color: var(--text-secondary);
      text-decoration: none;
      font-weight: var(--font-medium);
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
      
      &.active {
        background: linear-gradient(135deg, rgba(99, 102, 241, 0.15) 0%, rgba(99, 102, 241, 0.1) 100%);
        color: var(--color-primary-400);
        
        &::before {
          content: '';
          position: absolute;
          left: 0;
          top: 50%;
          transform: translateY(-50%);
          width: 3px;
          height: 24px;
          background: var(--color-primary-500);
          border-radius: 0 var(--radius-full) var(--radius-full) 0;
        }
      }
    }
    
    .nav-item {
      position: relative;
    }
    
    .nav-label {
      white-space: nowrap;
      overflow: hidden;
    }
    
    .collapsed .nav-link {
      justify-content: center;
      padding: var(--space-3);
    }
    
    .sidebar-footer {
      padding: var(--space-4);
      border-top: 1px solid var(--border-primary);
    }
    
    .collapse-btn {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 100%;
      height: 36px;
      background: var(--bg-tertiary);
      border-radius: var(--radius-md);
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--border-primary);
        color: var(--text-primary);
      }
    }
    
    .collapsed .collapse-btn {
      width: 36px;
      margin: 0 auto;
    }
    
    @media (max-width: 768px) {
      .sidebar {
        transform: translateX(-100%);
      }
      
      .sidebar.expanded {
        transform: translateX(0);
      }
    }
  `],
})
export class SidebarComponent {
  private readonly authService = inject(AuthService);
  private readonly document = inject(DOCUMENT);

  readonly collapsed = signal(false);

  readonly visibleNavItems = computed(() => {
    const user = this.authService.user();
    return NAV_ITEMS.filter(item => {
      if (!item.roles) return true;
      if (!user) return false;
      
      const roleMap: Record<number, string> = { 1: 'Customer', 2: 'Operator', 3: 'Admin' };
      const userRole = typeof user.role === 'number' ? roleMap[user.role] : user.role;
      
      return item.roles.includes(userRole);
    });
  });

  constructor() {
    effect(() => {
      const root = this.document.documentElement;
      if (this.collapsed()) {
        root.style.setProperty('--current-sidebar-width', 'var(--sidebar-collapsed-width)');
        root.classList.add('sidebar-collapsed');
      } else {
        root.style.setProperty('--current-sidebar-width', 'var(--sidebar-width)');
        root.classList.remove('sidebar-collapsed');
      }
    });
  }

  toggleCollapse(): void {
    this.collapsed.update(v => !v);
  }
}
