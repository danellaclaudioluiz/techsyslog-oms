import { Component, ChangeDetectionStrategy, input, computed } from '@angular/core';
import { IconComponent, IconName } from '../icon/icon.component';
import { OrderStatus, ORDER_STATUS_CONFIG } from '@core/models';

export type BadgeVariant = 'default' | 'primary' | 'success' | 'warning' | 'error' | 'info';
export type BadgeSize = 'sm' | 'md';

@Component({
  selector: 'app-badge',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span 
      class="badge"
      [class]="badgeClasses()"
      [style.--badge-color]="customColor()"
      [style.--badge-bg]="customBg()"
    >
      @if (icon()) {
        <app-icon [name]="icon()!" [size]="iconSize()" />
      }
      @if (showDot()) {
        <span class="dot"></span>
      }
      <span class="badge-text">
        <ng-content />
      </span>
    </span>
  `,
  styles: [`
    .badge {
      display: inline-flex;
      align-items: center;
      gap: var(--space-1);
      font-weight: var(--font-medium);
      border-radius: var(--radius-full);
      white-space: nowrap;
    }
    
    .size-sm {
      padding: 2px var(--space-2);
      font-size: 11px;
    }
    
    .size-md {
      padding: var(--space-1) var(--space-3);
      font-size: var(--text-xs);
    }
    
    .variant-default {
      background: var(--bg-tertiary);
      color: var(--text-secondary);
    }
    
    .variant-primary {
      background: rgba(99, 102, 241, 0.15);
      color: var(--color-primary-400);
    }
    
    .variant-success {
      background: rgba(16, 185, 129, 0.15);
      color: var(--color-success);
    }
    
    .variant-warning {
      background: rgba(245, 158, 11, 0.15);
      color: var(--color-warning);
    }
    
    .variant-error {
      background: rgba(239, 68, 68, 0.15);
      color: var(--color-error);
    }
    
    .variant-info {
      background: rgba(59, 130, 246, 0.15);
      color: var(--color-info);
    }
    
    .variant-custom {
      background: var(--badge-bg);
      color: var(--badge-color);
    }
    
    .dot {
      width: 6px;
      height: 6px;
      border-radius: 50%;
      background: currentColor;
    }
    
    .badge-text {
      line-height: 1.2;
    }
  `],
})
export class BadgeComponent {
  readonly variant = input<BadgeVariant>('default');
  readonly size = input<BadgeSize>('md');
  readonly icon = input<IconName>();
  readonly showDot = input(false);
  readonly customColor = input<string>();
  readonly customBg = input<string>();

  readonly badgeClasses = computed(() => {
    const classes = [`size-${this.size()}`];
    
    if (this.customColor() && this.customBg()) {
      classes.push('variant-custom');
    } else {
      classes.push(`variant-${this.variant()}`);
    }
    
    return classes.join(' ');
  });

  readonly iconSize = computed(() => this.size() === 'sm' ? 12 : 14);
}

@Component({
  selector: 'app-order-status-badge',
  standalone: true,
  imports: [BadgeComponent, IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-badge
      [customColor]="config().color"
      [customBg]="config().bgColor"
      [size]="size()"
    >
      <app-icon [name]="config().icon" [size]="iconSize()" />
      {{ config().label }}
    </app-badge>
  `,
})
export class OrderStatusBadgeComponent {
  readonly status = input.required<OrderStatus | number>();
  readonly size = input<BadgeSize>('md');

  readonly config = computed(() => {
    const status = this.status();
    
    // Se não tiver status, retorna Pending
    if (status === undefined || status === null) {
      return ORDER_STATUS_CONFIG['Pending'];
    }
    
    // Se for número, converte para string
    if (typeof status === 'number') {
      const statusMap: Record<number, OrderStatus> = {
        1: 'Pending',
        2: 'Confirmed',
        3: 'InTransit',
        4: 'Delivered',
        5: 'Cancelled',
      };
      const statusString = statusMap[status] || 'Pending';
      return ORDER_STATUS_CONFIG[statusString];
    }
    
    // Se for string, usa direto
    return ORDER_STATUS_CONFIG[status] || ORDER_STATUS_CONFIG['Pending'];
  });
  
  readonly iconSize = computed(() => this.size() === 'sm' ? 12 : 14);
}