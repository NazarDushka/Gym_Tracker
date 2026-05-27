import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token'); 
  const router = inject(Router);

  let authReq = req;

  if (token) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }



  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {

        localStorage.removeItem('token');
        router.navigate(['/login']);
      }

      return throwError(() => error);

    })
  );
}