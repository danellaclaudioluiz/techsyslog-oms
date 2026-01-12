import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '@core/services';

// ============================================================================
// Error Interceptor (Functional)
// Global error handling and user feedback
// ============================================================================

export const errorInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const toastService = inject(ToastService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Don't show toast for specific cases
      const skipToast = 
        error.status === 401 || // Auth errors handled separately
        error.status === 404 || // Not found often expected
        req.headers.has('X-Skip-Error-Toast'); // Manual skip

      if (!skipToast) {
        const message = getErrorMessage(error);
        toastService.error(message);
      }

      return throwError(() => error);
    })
  );
};

function getErrorMessage(error: HttpErrorResponse): string {
  // API error with message
  if (error.error?.message) {
    return error.error.message;
  }

  // API error with errors array
  if (error.error?.errors?.length) {
    return error.error.errors[0];
  }

  // Standard HTTP errors
  switch (error.status) {
    case 0:
      return 'Não foi possível conectar ao servidor. Verifique sua conexão.';
    case 400:
      return 'Dados inválidos. Verifique as informações e tente novamente.';
    case 403:
      return 'Você não tem permissão para realizar esta ação.';
    case 404:
      return 'Recurso não encontrado.';
    case 409:
      return 'Conflito de dados. O recurso já existe.';
    case 422:
      return 'Dados inválidos. Verifique as informações.';
    case 429:
      return 'Muitas requisições. Aguarde um momento.';
    case 500:
      return 'Erro interno do servidor. Tente novamente mais tarde.';
    case 502:
    case 503:
    case 504:
      return 'Servidor temporariamente indisponível. Tente novamente.';
    default:
      return 'Ocorreu um erro inesperado. Tente novamente.';
  }
}
