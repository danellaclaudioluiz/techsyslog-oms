import { Injectable, signal, computed } from '@angular/core';

// ============================================================================
// Toast Service
// Simple toast notification system
// ============================================================================

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  readonly id: string;
  readonly type: ToastType;
  readonly title?: string;
  readonly message: string;
  readonly duration: number;
  readonly dismissible: boolean;
}

export interface ToastOptions {
  title?: string;
  duration?: number;
  dismissible?: boolean;
}

const DEFAULT_DURATION = 5000;

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly _toasts = signal<Toast[]>([]);
  private toastId = 0;

  readonly toasts = this._toasts.asReadonly();
  readonly hasToasts = computed(() => this._toasts().length > 0);

  // === Public Methods ===

  success(message: string, options?: ToastOptions): string {
    return this.show('success', message, options);
  }

  error(message: string, options?: ToastOptions): string {
    return this.show('error', message, { ...options, duration: options?.duration ?? 8000 });
  }

  warning(message: string, options?: ToastOptions): string {
    return this.show('warning', message, options);
  }

  info(message: string, options?: ToastOptions): string {
    return this.show('info', message, options);
  }

  dismiss(id: string): void {
    this._toasts.update(toasts => toasts.filter(t => t.id !== id));
  }

  dismissAll(): void {
    this._toasts.set([]);
  }

  // === Private Methods ===

  private show(type: ToastType, message: string, options?: ToastOptions): string {
    const id = `toast-${++this.toastId}`;
    const duration = options?.duration ?? DEFAULT_DURATION;

    const toast: Toast = {
      id,
      type,
      title: options?.title,
      message,
      duration,
      dismissible: options?.dismissible ?? true,
    };

    this._toasts.update(toasts => [...toasts, toast]);

    // Auto dismiss
    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }

    return id;
  }
}
