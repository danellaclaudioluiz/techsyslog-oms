import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '@core/services';
import { environment } from '@env/environment';

// ============================================================================
// Auth Interceptor (Functional)
// Adds JWT token to requests and handles 401 errors
// ============================================================================

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<unknown>,
  next: HttpHandlerFn
) => {
  const authService = inject(AuthService);
  const token = authService.token();

  // Only add token for requests to our API
  const isApiRequest = req.url.startsWith(environment.apiUrl);
  
  if (token && isApiRequest) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`,
      },
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized
      if (error.status === 401 && isApiRequest) {
        authService.logout();
      }
      return throwError(() => error);
    })
  );
};
