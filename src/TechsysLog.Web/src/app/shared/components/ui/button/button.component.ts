import { Component, ChangeDetectionStrategy, input, output, computed } from '@angular/core';
import { IconComponent, IconName } from '../icon/icon.component';

// ============================================================================
// Button Component
// Configurable button with multiple variants and states
// ============================================================================

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger' | 'success';
export type ButtonSize = 'sm' | 'md' | 'lg';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      [type]="type()"
      [disabled]="disabled() || loading()"
      [class]="buttonClasses()"
      (click)="handleClick($event)"
    >
      @if (loading()) {
        <app-icon name="loader" [size]="iconSize()" className="spin" />
      } @else if (iconLeft()) {
        <app-icon [name]="iconLeft()!" [size]="iconSize()" />
      }
      
      <span class="button-text">
        <ng-content />
      </span>
      
      @if (iconRight() && !loading()) {
        <app-icon [name]="iconRight()!" [size]="iconSize()" />
      }
    </button>
  `,
  styles: [`
    :host {
      display: inline-block;
    }
    
    button {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: var(--space-2);
      font-family: var(--font-sans);
      font-weight: var(--font-medium);
      border-radius: var(--radius-lg);
      border: 1px solid transparent;
      cursor: pointer;
      transition: all var(--transition-fast);
      white-space: nowrap;
      
      &:focus-visible {
        outline: 2px solid var(--color-primary-500);
        outline-offset: 2px;
      }
      
      &:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    }
    
    .button-text {
      line-height: 1;
    }
    
    /* Sizes */
    .size-sm {
      height: 32px;
      padding: 0 var(--space-3);
      font-size: var(--text-sm);
    }
    
    .size-md {
      height: 40px;
      padding: 0 var(--space-4);
      font-size: var(--text-sm);
    }
    
    .size-lg {
      height: 48px;
      padding: 0 var(--space-6);
      font-size: var(--text-base);
    }
    
    /* Variants */
    .variant-primary {
      background: linear-gradient(135deg, var(--color-primary-500) 0%, var(--color-primary-600) 100%);
      color: white;
      box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1), 
                  0 4px 12px rgba(99, 102, 241, 0.25);
      
      &:hover:not(:disabled) {
        background: linear-gradient(135deg, var(--color-primary-400) 0%, var(--color-primary-500) 100%);
        transform: translateY(-1px);
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1), 
                    0 8px 16px rgba(99, 102, 241, 0.3);
      }
      
      &:active:not(:disabled) {
        transform: translateY(0);
      }
    }
    
    .variant-secondary {
      background: var(--bg-secondary);
      color: var(--text-primary);
      border-color: var(--border-primary);
      
      &:hover:not(:disabled) {
        background: var(--bg-tertiary);
        border-color: var(--color-neutral-600);
      }
    }
    
    .variant-ghost {
      background: transparent;
      color: var(--text-secondary);
      
      &:hover:not(:disabled) {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
    }
    
    .variant-danger {
      background: var(--color-error);
      color: white;
      
      &:hover:not(:disabled) {
        background: #dc2626;
      }
    }
    
    .variant-success {
      background: var(--color-success);
      color: white;
      
      &:hover:not(:disabled) {
        background: #059669;
      }
    }
    
    /* Full width */
    .full-width {
      width: 100%;
    }
    
    /* Icon only */
    .icon-only {
      padding: 0;
      
      &.size-sm { width: 32px; }
      &.size-md { width: 40px; }
      &.size-lg { width: 48px; }
    }
  `],
})
export class ButtonComponent {
  readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
  readonly type = input<'button' | 'submit' | 'reset'>('button');
  readonly disabled = input(false);
  readonly loading = input(false);
  readonly fullWidth = input(false);
  readonly iconOnly = input(false);
  readonly iconLeft = input<IconName>();
  readonly iconRight = input<IconName>();

  readonly clicked = output<MouseEvent>();

  readonly buttonClasses = computed(() => {
    const classes = [
      `variant-${this.variant()}`,
      `size-${this.size()}`,
    ];
    
    if (this.fullWidth()) classes.push('full-width');
    if (this.iconOnly()) classes.push('icon-only');
    
    return classes.join(' ');
  });

  readonly iconSize = computed(() => {
    switch (this.size()) {
      case 'sm': return 16;
      case 'lg': return 20;
      default: return 18;
    }
  });

  handleClick(event: MouseEvent): void {
    if (!this.disabled() && !this.loading()) {
      this.clicked.emit(event);
    }
  }
}
