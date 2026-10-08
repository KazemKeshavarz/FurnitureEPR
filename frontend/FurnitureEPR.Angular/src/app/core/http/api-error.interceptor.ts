import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

export const apiErrorInterceptor: HttpInterceptorFn = (request, next) =>
  next(request).pipe(
    catchError((error: HttpErrorResponse) => {
      // پیام خطا در لایه UI نمایش داده می‌شود؛ این interceptor فقط خطا را عبور می‌دهد.
      return throwError(() => error);
    })
  );
