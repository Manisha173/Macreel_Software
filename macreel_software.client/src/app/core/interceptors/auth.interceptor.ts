import {
  HttpClient,
  HttpErrorResponse,
  HttpInterceptorFn,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  filter,
  take,
  switchMap,
  throwError,
  BehaviorSubject,
} from 'rxjs';
import { environment } from '../../../environments/environment';
import { jwtDecode } from 'jwt-decode';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshSubject = new BehaviorSubject<boolean>(false);
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const http = inject(HttpClient);
  const router = inject(Router);
  const baseUrl = environment.apiUrl;

  const auth = inject(AuthService);

  // Always send cookies
  const authReq = req.clone({
    withCredentials: true,
  });

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // If refresh already running → wait
        if (isRefreshing) {
          return refreshSubject.pipe(
            filter((done) => done),
            take(1),
            switchMap(() => next(authReq)),
          );
        }

        // Start refresh flow
        isRefreshing = true;
        refreshSubject.next(false);

        return http
          .post(`${baseUrl}Auth/refresh`, {}, { withCredentials: true })
          .pipe(
            switchMap((res: any) => {
              isRefreshing = false;
              refreshSubject.next(true);
              const decodeToken: any = jwtDecode(res.token);
              const userRole =
                decodeToken.role ||
                decodeToken[
                  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'
                ];
              auth.setRole(userRole);
              return next(authReq); // retry original request
            }),
            catchError((err) => {
              isRefreshing = false;
              router.navigate(['/login']);
              return throwError(() => err);
            }),
          );
      }
      return throwError(() => error);
    }),
  );
};
