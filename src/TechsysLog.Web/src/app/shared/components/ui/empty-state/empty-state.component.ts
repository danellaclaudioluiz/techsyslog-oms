import { Component, ChangeDetectionStrategy, input } from '@angular/core';
import { IconComponent, IconName } from '../icon/icon.component';
import { ButtonComponent } from '../button/button.component';

// ============================================================================
// Empty State Component
// Placeholder for empty lists/states
// ============================================================================

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [IconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="empty-state">
      <div class="empty-state-icon">
        <app-icon [name]="icon()" [size]="48" />
      </div>
      
      <h3 class="empty-state-title">{{ title() }}</h3>
      
      @if (description()) {
        <p class="empty-state-description">{{ description() }}</p>
      }
      
      <div class="empty-state-actions">
        <ng-content />
      </div>
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      text-align: center;
      padding: var(--space-12) var(--space-6);
      max-width: 400px;
      margin: 0 auto;
    }
    
    .empty-state-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      height: 80px;
      border-radius: var(--radius-2xl);
      background: var(--bg-tertiary);
      color: var(--text-muted);
      margin-bottom: var(--space-6);
    }
    
    .empty-state-title {
      font-size: var(--text-xl);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
      margin: 0 0 var(--space-2);
    }
    
    .empty-state-description {
      font-size: var(--text-base);
      color: var(--text-secondary);
      margin: 0 0 var(--space-6);
      line-height: var(--leading-relaxed);
    }
    
    .empty-state-actions {
      display: flex;
      gap: var(--space-3);
    }
  `],
})
export class EmptyStateComponent {
  readonly icon = input<IconName>('package');
  readonly title = input.required<string>();
  readonly description = input<string>();
}
