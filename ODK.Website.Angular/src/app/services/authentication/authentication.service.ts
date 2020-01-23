import { HttpClient, HttpParams, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable, of, Subject, BehaviorSubject } from 'rxjs';
import { map, take, tap } from 'rxjs/operators';

import { AccountDetails } from 'src/app/core/account/account-details';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { catchApiError } from '../http/catchApiError';
import { environment } from 'src/environments/environment';
import { HttpAuthInterceptorHeaders } from '../http/http-auth-interceptor-headers';
import { HttpUtils } from '../http/http-utils';
import { ServiceResult } from '../service-result';
import { StorageService } from '../caching/storage.service';

const baseUrl = `${environment.apiBaseUrl}/account`;

const endpoints = {
  changePassword: `${baseUrl}/password`,
  completePasswordReset: `${baseUrl}/password/reset/complete`,
  login: `${baseUrl}/login`,
  refreshToken: `${baseUrl}/refreshToken`,
  requestPasswordReset: `${baseUrl}/password/reset/request`
}

const storageKeys = {
  authToken: '_auth.token'
};

@Injectable({
  providedIn: 'root'
})
export class AuthenticationService {

  constructor(private http: HttpClient,
    private storageService: StorageService
  ) {
    const token: AuthenticationToken = this.getToken();
    this.tokenSubject = new BehaviorSubject<AuthenticationToken>(token);
  }

  private loginSubject: Subject<ServiceResult<AuthenticationToken>> = new Subject<ServiceResult<AuthenticationToken>>();
  private nextTokenSubject: Subject<AuthenticationToken> = new Subject<AuthenticationToken>();
  private refreshSubject: Subject<ServiceResult<AuthenticationToken>> = new Subject<ServiceResult<AuthenticationToken>>();
  private tokenExpiredSubject: Subject<AuthenticationToken> = new Subject<AuthenticationToken>();
  private tokenSubject: Subject<AuthenticationToken>;

  authenticationExpired(): Observable<AuthenticationToken> {
    return this.tokenExpiredSubject.asObservable();
  }

  authenticationTokenChange(): Observable<AuthenticationToken> {
    return this.tokenSubject.asObservable();
  }

  changePassword(currentPassword: string, newPassword: string): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      currentPassword,
      newPassword
    });

    return this.http.put(endpoints.changePassword, params)
      .pipe(
        map((): ServiceResult<void> => ({ success: true })),
        catchApiError()
      );
  }

  completePasswordReset(password: string, token: string): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      password,
      token
    });

    return this.http.post(endpoints.completePasswordReset, params).pipe(
      map((): ServiceResult<void> => ({ success: true })),
      catchApiError()
    )
  }
  
  getAccountDetails(): AccountDetails {
    const token: AuthenticationToken = this.getToken();
    return token ? {      
      adminChapterIds: token.adminChapterIds || [],
      chapterId: token.chapterId,
      memberId: token.memberId,
      membershipActive: !token.membershipDisabled
    } : null;
  }

  getNextToken(): Observable<AuthenticationToken> {
    return this.nextTokenSubject
      .pipe(
        take(1)
      );
  }

  getToken(): AuthenticationToken {
    return this.storageService.get<AuthenticationToken>(storageKeys.authToken);
  }

  login(username: string, password: string): Observable<ServiceResult<AuthenticationToken>> {
    const params: HttpParams = HttpUtils.createFormParams({
      username: username,
      password: password
    });

    this.requestToken(endpoints.login, {
      params,
      subject: this.loginSubject
    });

    return this.loginSubject
      .pipe(
        take(1),
        map((result: ServiceResult<AuthenticationToken>) => {
          return result.success ? result : {
            messages: result.messages,
            success: false
          };
        })
      );
  }

  logout(): Observable<void> {
    const token: AuthenticationToken = this.getToken();
    if (!token) {
      return of();
    }

    const params: HttpParams = HttpUtils.createFormParams({
      refreshToken: token.refreshToken
    });

    return this.http.delete(endpoints.refreshToken, { params }).pipe(
      tap(() => this.setToken({ success: false })),
      map(() => undefined)
    );
  }

  refreshAccessToken(token: AuthenticationToken): Observable<ServiceResult<AuthenticationToken>> {
    if (!token) {
      return of(null);
    }

    const params: HttpParams = HttpUtils.createFormParams({
      refreshToken: token.refreshToken
    });

    const headers: HttpHeaders = new HttpAuthInterceptorHeaders({
      isTokenRefresh: true
    });

    this.requestToken(endpoints.refreshToken, {
      headers,
      params,
      subject: this.refreshSubject
    });

    return this.refreshSubject
      .pipe(
        take(1),        
        map((result: ServiceResult<AuthenticationToken>) => {
          return result.success ? result : {
            messages: result.messages,
            success: false            
          };
        }),
        tap((result: ServiceResult<AuthenticationToken>) => {
          if (!result.success) {
            this.setToken(result);
            this.tokenExpiredSubject.next(result.value);
          }
        })
      );
  }

  requestPasswordReset(emailAddress: string): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      emailAddress
    });

    return this.http.post(endpoints.requestPasswordReset, params)
      .pipe(
        map(() => undefined)
      );
  }

  private mapToken(response: any): AuthenticationToken {
    const token: AuthenticationToken = {
      accessToken: response.accessToken,
      chapterId: response.chapterId,
      memberId: response.memberId,
      membershipDisabled: response.membershipDisabled || false,
      refreshToken: response.refreshToken,
      subscriptionExpiryDate: response.subscriptionExpiryDate ? new Date(response.subscriptionExpiryDate) : null
    };

    if (response.adminChapterIds) {
      token.adminChapterIds = response.adminChapterIds;
    }

    if (response.superAdmin) {
      token.superAdmin = response.superAdmin;
    }

    return token;
  }

  private requestToken(url: string, options: {
    headers?: HttpHeaders,
    params: HttpParams,
    subject: Subject<ServiceResult<AuthenticationToken>>
  }): void {
    this.http
      .post(url, options.params, { headers: options.headers })
      .pipe(
        map(response => this.mapToken(response)),
        map((token: AuthenticationToken): ServiceResult<AuthenticationToken> => ({
          success: true,
          value: token
        })),
        catchApiError(this.getToken())
      ).subscribe((result: ServiceResult<AuthenticationToken>) => {
        this.setToken(result, options.subject);
      });
  }

  private setStorageToken(token: AuthenticationToken): void {
    this.storageService.set(storageKeys.authToken, token);
  }

  private setToken(result: ServiceResult<AuthenticationToken>, subject?: Subject<ServiceResult<AuthenticationToken>>): void {
    const token: AuthenticationToken = result.success ? result.value : null;
    this.setStorageToken(token);

    if (subject) {
      subject.next(result);
    }

    this.nextTokenSubject.next(token);
    this.tokenSubject.next(token);
  }
}
