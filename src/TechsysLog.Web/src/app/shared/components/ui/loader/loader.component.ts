import { Component, ChangeDetectionStrategy, input } from '@angular/core';

// ============================================================================
// Loader Component
// Loading spinner with optional text
// ============================================================================

@Component({
  selector: 'app-loader',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="loader-wrapper" [class]="'size-' + size()">
      <div class="loader-spinner">
        <svg viewBox="0 0 50 50">
          <circle
            cx="25"
            cy="25"
            r="20"
            fill="none"
            stroke="currentColor"
            stroke-width="4"
            stroke-linecap="round"
          />
        </svg>
      </div>
      @if (text()) {
        <span class="loader-text">{{ text() }}</span>
      }
    </div>
  `,
  styles: [`
    .loader-wrapper {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      gap: var(--space-3);
    }
    
    .loader-spinner {
      animation: spin 1.2s linear infinite;
      color: var(--color-primary-500);
      
      svg {
        display: block;
      }
      
      circle {
        stroke-dasharray: 90, 150;
        stroke-dashoffset: 0;
        animation: dash 1.5s ease-in-out infinite;
      }
    }
    
    @keyframes spin {
      100% { transform: rotate(360deg); }
    }
    
    @keyframes dash {
      0% {
        stroke-dasharray: 1, 150;
        stroke-dashoffset: 0;
      }
      50% {
        stroke-dasharray: 90, 150;
        stroke-dashoffset: -35;
      }
      100% {
        stroke-dasharray: 90, 150;
        stroke-dashoffset: -124;
      }
    }
    
    .loader-text {
      font-size: var(--text-sm);
      color: var(--text-secondary);
    }
    
    /* Sizes */
    .size-sm .loader-spinner {
      width: 20px;
      height: 20px;
    }
    
    .size-md .loader-spinner {
      width: 32px;
      height: 32px;
    }
    
    .size-lg .loader-spinner {
      width: 48px;
      height: 48px;
    }
    
    .size-xl .loader-spinner {
      width: 64px;
      height: 64px;
    }
  `],
})
export class LoaderComponent {
  readonly size = input<'sm' | 'md' | 'lg' | 'xl'>('md');
  readonly text = input<string>();
}

// ============================================================================
// Page Loader Component
// Full page loading overlay
// ============================================================================

@Component({
  selector: 'app-page-loader',
  standalone: true,
  imports: [LoaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="page-loader">
      <app-loader size="lg" [text]="text()" />
    </div>
  `,
  styles: [`
    .page-loader {
      position: fixed;
      inset: 0;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--bg-primary);
      z-index: 1000;
    }
  `],
})
export class PageLoaderComponent {
  readonly text = input<string>();
}
