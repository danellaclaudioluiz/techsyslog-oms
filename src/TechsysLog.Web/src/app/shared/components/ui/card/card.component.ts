import { Component, ChangeDetectionStrategy, input } from '@angular/core';

// ============================================================================
// Card Component
// Container with consistent styling
// ============================================================================

@Component({
  selector: 'app-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div 
      class="card" 
      [class.hoverable]="hoverable()" 
      [class.clickable]="clickable()"
      [class.glass]="glass()"
      [class.no-padding]="noPadding()"
    >
      @if (title() || subtitle()) {
        <div class="card-header">
          @if (title()) {
            <h3 class="card-title">{{ title() }}</h3>
          }
          @if (subtitle()) {
            <p class="card-subtitle">{{ subtitle() }}</p>
          }
          <div class="card-header-actions">
            <ng-content select="[card-actions]" />
          </div>
        </div>
      }
      
      <div class="card-content">
        <ng-content />
      </div>
      
      <ng-content select="[card-footer]" />
    </div>
  `,
  styles: [`
    .card {
      background: var(--bg-secondary);
      border: 1px solid var(--border-primary);
      border-radius: var(--radius-xl);
      overflow: hidden;
      transition: all var(--transition-base);
    }
    
    .card-header {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-5) var(--space-6);
      border-bottom: 1px solid var(--border-primary);
    }
    
    .card-title {
      font-size: var(--text-lg);
      font-weight: var(--font-semibold);
      color: var(--text-primary);
      margin: 0;
    }
    
    .card-subtitle {
      font-size: var(--text-sm);
      color: var(--text-secondary);
      margin: 0;
      flex-basis: 100%;
    }
    
    .card-header-actions {
      margin-left: auto;
    }
    
    .card-content {
      padding: var(--space-6);
    }
    
    .no-padding .card-content {
      padding: 0;
    }
    
    .hoverable:hover {
      border-color: var(--color-neutral-600);
      box-shadow: var(--shadow-md);
    }
    
    .clickable {
      cursor: pointer;
      
      &:hover {
        border-color: var(--color-primary-500);
        box-shadow: var(--shadow-glow);
      }
    }
    
    .glass {
      background: var(--bg-elevated);
      backdrop-filter: blur(12px);
      -webkit-backdrop-filter: blur(12px);
    }
  `],
})
export class CardComponent {
  readonly title = input<string>();
  readonly subtitle = input<string>();
  readonly hoverable = input(false);
  readonly clickable = input(false);
  readonly glass = input(false);
  readonly noPadding = input(false);
}
