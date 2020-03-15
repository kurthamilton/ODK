import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpHeaders, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, EMPTY, throwError } from 'rxjs';
import { switchMap, catchError } from 'rxjs/operators';

import { AuthenticationService } from '../authentication/authentication.service';
import { AuthenticationToken } from '../../core/authentication/authentication-token';
import { HttpAuthInterceptorHeaders } from './http-auth-interceptor-headers';
import { HttpAuthInterceptorOptions } from './http-auth-interceptor-options';

@Injectable({
  providedIn: 'root'
})
export class HttpAuthInterceptor implements HttpInterceptor {

  constructor(private authenticationService: AuthenticationService) {}

  private isRefreshing = false;
  
  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token: AuthenticationToken = this.authenticationService.getToken();
    return this.handle(request, next, token);
  }

  private handle(request: HttpRequest<any>, next: HttpHandler, token: AuthenticationToken): Observable<HttpEvent<any>> {
    request = this.setAuthenticationHeader(request, token);
    request = this.removeInterceptorHeaders(request);

    return next.handle(request).pipe(
      catchError((err: any) => this.handleError(request, next, token, err))
    );
  }

  private handleError(request: HttpRequest<any>, next: HttpHandler, token: AuthenticationToken,
    err: HttpErrorResponse
  ): Observable<HttpEvent<any>> {

    if (err.status !== 401 || !token) {
      return throwError(err);
    }

    if (!this.isRefreshing) {
      this.isRefreshing = true;

      this.authenticationService
        .refreshAccessToken(token)
        .subscribe(() => this.isRefreshing = false);
    }

    return this.authenticationService.getNextToken()
      .pipe(
        switchMap((refreshed: AuthenticationToken) => this.handleRefreshedToken(request, next, refreshed))
      );
  }

  private handleRefreshedToken(request: HttpRequest<any>, next: HttpHandler, token: AuthenticationToken): Observable<HttpEvent<any>> {
    if (!token) {
      return EMPTY;
    }

    request = this.setInterceptorHeaders(request, {
      isRetry: true
    });

    return this.handle(request, next, token);
  }

  private removeInterceptorHeaders(request: HttpRequest<any>): HttpRequest<any> {
    const headers: HttpHeaders = HttpAuthInterceptorHeaders.removeInterceptorHeaders(request.headers);

    return request.clone({
      headers: headers
    });
  }

  private setAuthenticationHeader(request: HttpRequest<any>, token: AuthenticationToken): HttpRequest<any> {
    return token ? request.clone({
      setHeaders: {
        Authorization: `Bearer ${token.accessToken}`
      }
    }) : request;
  }

  private setInterceptorHeaders(request: HttpRequest<any>, options: HttpAuthInterceptorOptions): HttpRequest<any> {
    const headers: HttpHeaders = HttpAuthInterceptorHeaders.setInterceptorHeaders(request.headers, options);

    return request.clone({
      headers: headers
    });
  }
}
