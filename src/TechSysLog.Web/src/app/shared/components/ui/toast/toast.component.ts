import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { animate, style, transition, trigger } from '@angular/animations';
import { ToastService, Toast } from '@core/services';
import { IconComponent, IconName } from '../icon/icon.component';

// ============================================================================
// Toast Container Component
// Displays toast notifications
// ============================================================================

@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('toastAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(100%)' }),
        animate('300ms cubic-bezier(0.34, 1.56, 0.64, 1)', 
          style({ opacity: 1, transform: 'translateX(0)' })),
      ]),
      transition(':leave', [
        animate('200ms ease-in', 
          style({ opacity: 0, transform: 'translateX(100%)' })),
      ]),
    ]),
  ],
  template: `
    <div class="toast-container">
      @for (toast of toastService.toasts(); track toast.id) {
        <div 
          class="toast"
          [class]="'toast-' + toast.type"
          [@toastAnimation]
        >
          <div class="toast-icon">
            <app-icon [name]="getIcon(toast.type)" [size]="20" />
          </div>
          
          <div class="toast-content">
            @if (toast.title) {
              <div class="toast-title">{{ toast.title }}</div>
            }
            <div class="toast-message">{{ toast.message }}</div>
          </div>
          
          @if (toast.dismissible) {
            <button 
              class="toast-close"
              (click)="dismiss(toast.id)"
              aria-label="Fechar"
            >
              <app-icon name="x" [size]="16" />
            </button>
          }
          
          <div class="toast-progress" [style.animation-duration.ms]="toast.duration"></div>
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container {
      position: fixed;
      top: var(--space-6);
      right: var(--space-6);
      z-index: var(--z-toast);
      display: flex;
      flex-direction: column;
      gap: var(--space-3);
      max-width: 400px;
      width: 100%;
      pointer-events: none;
    }
    
    .toast {
      display: flex;
      align-items: flex-start;
      gap: var(--space-3);
      padding: var(--space-4);
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-lg);
      box-shadow: var(--shadow-lg);
      pointer-events: auto;
      position: relative;
      overflow: hidden;
    }
    
    .toast-icon {
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 32px;
      height: 32px;
      border-radius: var(--radius-md);
    }
    
    .toast-content {
      flex: 1;
      min-width: 0;
    }
    
    .toast-title {
      font-weight: var(--font-semibold);
      color: var(--text-primary);
      margin-bottom: var(--space-1);
    }
    
    .toast-message {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      line-height: var(--leading-relaxed);
    }
    
    .toast-close {
      flex-shrink: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      width: 24px;
      height: 24px;
      border-radius: var(--radius-sm);
      background: transparent;
      border: none;
      color: var(--text-muted);
      cursor: pointer;
      transition: all var(--transition-fast);
      
      &:hover {
        background: var(--bg-tertiary);
        color: var(--text-primary);
      }
    }
    
    .toast-progress {
      position: absolute;
      bottom: 0;
      left: 0;
      height: 3px;
      width: 100%;
      animation: progress linear forwards;
    }
    
    @keyframes progress {
      from { width: 100%; }
      to { width: 0; }
    }
    
    /* Variants */
    .toast-success {
      border-left: 3px solid var(--color-success);
      
      .toast-icon {
        background: rgba(16, 185, 129, 0.15);
        color: var(--color-success);
      }
      
      .toast-progress {
        background: var(--color-success);
      }
    }
    
    .toast-error {
      border-left: 3px solid var(--color-error);
      
      .toast-icon {
        background: rgba(239, 68, 68, 0.15);
        color: var(--color-error);
      }
      
      .toast-progress {
        background: var(--color-error);
      }
    }
    
    .toast-warning {
      border-left: 3px solid var(--color-warning);
      
      .toast-icon {
        background: rgba(245, 158, 11, 0.15);
        color: var(--color-warning);
      }
      
      .toast-progress {
        background: var(--color-warning);
      }
    }
    
    .toast-info {
      border-left: 3px solid var(--color-info);
      
      .toast-icon {
        background: rgba(59, 130, 246, 0.15);
        color: var(--color-info);
      }
      
      .toast-progress {
        background: var(--color-info);
      }
    }
    
    @media (max-width: 480px) {
      .toast-container {
        left: var(--space-4);
        right: var(--space-4);
        max-width: none;
      }
    }
  `],
})
export class ToastContainerComponent {
  readonly toastService = inject(ToastService);

  getIcon(type: string): IconName {
    const icons: Record<string, IconName> = {
      success: 'check-circle',
      error: 'alert-circle',
      warning: 'alert-circle',
      info: 'info',
    };
    return icons[type] || 'info';
  }

  dismiss(id: string): void {
    this.toastService.dismiss(id);
  }
}
