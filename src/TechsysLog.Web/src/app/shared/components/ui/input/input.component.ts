import { Component, ChangeDetectionStrategy, input, output, signal, forwardRef, computed } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, FormsModule } from '@angular/forms';
import { IconComponent, IconName } from '../icon/icon.component';

// ============================================================================
// Input Component
// Form input with label, validation, and icons
// ============================================================================

@Component({
  selector: 'app-input',
  standalone: true,
  imports: [FormsModule, IconComponent],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true,
    },
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="input-wrapper" [class.has-error]="error()" [class.disabled]="disabled()">
      @if (label()) {
        <label [for]="inputId" class="input-label">
          {{ label() }}
          @if (required()) {
            <span class="required">*</span>
          }
        </label>
      }
      
      <div class="input-container" [class.focused]="focused()">
        @if (iconLeft()) {
          <span class="input-icon left">
            <app-icon [name]="iconLeft()!" [size]="18" />
          </span>
        }
        
        <input
          [id]="inputId"
          [type]="actualType()"
          [placeholder]="placeholder()"
          [disabled]="disabled()"
          [readonly]="readonly()"
          [autocomplete]="autocomplete()"
          [value]="value()"
          [class.has-icon-left]="iconLeft()"
          [class.has-icon-right]="iconRight() || type() === 'password'"
          (input)="onInput($event)"
          (focus)="onFocus()"
          (blur)="onBlur()"
        />
        
        @if (type() === 'password') {
          <button
            type="button"
            class="input-icon right toggle-password"
            (click)="togglePassword()"
            tabindex="-1"
          >
            <app-icon [name]="showPassword() ? 'eye-off' : 'eye'" [size]="18" />
          </button>
        } @else if (iconRight()) {
          <span class="input-icon right">
            <app-icon [name]="iconRight()!" [size]="18" />
          </span>
        }
      </div>
      
      @if (error()) {
        <span class="input-error">{{ error() }}</span>
      } @else if (hint()) {
        <span class="input-hint">{{ hint() }}</span>
      }
    </div>
  `,
  styles: [`
    .input-wrapper {
      display: flex;
      flex-direction: column;
      gap: var(--space-2);
    }
    
    .input-label {
      font-size: var(--text-sm);
      font-weight: var(--font-medium);
      color: var(--text-primary);
      
      .required {
        color: var(--color-error);
        margin-left: 2px;
      }
    }
    
    .input-container {
      position: relative;
      display: flex;
      align-items: center;
      background: var(--input-bg);
      border: 1px solid var(--input-border);
      border-radius: var(--radius-lg);
      transition: all var(--transition-fast);
      
      &:hover:not(.disabled) {
        border-color: var(--color-neutral-500);
      }
      
      &.focused {
        border-color: var(--input-focus);
        box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.15);
      }
    }
    
    input {
      flex: 1;
      width: 100%;
      height: 44px;
      padding: 0 var(--space-4);
      background: transparent;
      border: none;
      color: var(--text-primary);
      font-size: var(--text-base);
      
      &::placeholder {
        color: var(--text-muted);
      }
      
      &:focus {
        outline: none;
      }
      
      &:disabled {
        cursor: not-allowed;
        opacity: 0.6;
      }
      
      &.has-icon-left {
        padding-left: var(--space-10);
      }
      
      &.has-icon-right {
        padding-right: var(--space-10);
      }
    }
    
    .input-icon {
      position: absolute;
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--text-muted);
      
      &.left {
        left: var(--space-3);
      }
      
      &.right {
        right: var(--space-3);
      }
    }
    
    .toggle-password {
      background: none;
      border: none;
      cursor: pointer;
      padding: var(--space-1);
      border-radius: var(--radius-sm);
      
      &:hover {
        color: var(--text-secondary);
        background: var(--bg-tertiary);
      }
    }
    
    .input-error {
      font-size: var(--text-sm);
      color: var(--color-error);
    }
    
    .input-hint {
      font-size: var(--text-sm);
      color: var(--text-muted);
    }
    
    .has-error .input-container {
      border-color: var(--color-error);
      
      &.focused {
        box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.15);
      }
    }
    
    .disabled {
      opacity: 0.6;
      pointer-events: none;
    }
  `],
})
export class InputComponent implements ControlValueAccessor {
  readonly label = input<string>();
  readonly placeholder = input('');
  readonly type = input<'text' | 'email' | 'password' | 'number' | 'tel' | 'search'>('text');
  readonly hint = input<string>();
  readonly error = input<string>();
  readonly required = input(false);
  readonly disabled = input(false);
  readonly readonly = input(false);
  readonly autocomplete = input<string>('off');
  readonly iconLeft = input<IconName>();
  readonly iconRight = input<IconName>();

  readonly inputId = `input-${Math.random().toString(36).slice(2, 9)}`;

  readonly value = signal('');
  readonly focused = signal(false);
  readonly showPassword = signal(false);

  readonly actualType = computed(() => {
    if (this.type() === 'password' && this.showPassword()) {
      return 'text';
    }
    return this.type();
  });

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};

  // ControlValueAccessor implementation
  writeValue(value: string): void {
    this.value.set(value || '');
  }

  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    // Handled via input
  }

  // Event handlers
  onInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    const value = target.value;
    this.value.set(value);
    this.onChange(value);
  }

  onFocus(): void {
    this.focused.set(true);
  }

  onBlur(): void {
    this.focused.set(false);
    this.onTouched();
  }

  togglePassword(): void {
    this.showPassword.update(v => !v);
  }
}
