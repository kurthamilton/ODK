import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { catchApiError } from '../http/catchApiError';
import { environment } from 'src/environments/environment';
import { ServiceResult } from '../service-result';

const baseUrl = `${environment.apiBaseUrl}/admin/socialmedia`;

const endpoints = {
  instagramLogin: (chapterId: string) => `${baseUrl}/instagram/login?chapterId=${chapterId}`,
  instagramSendVerifyCode: (chapterId: string, code: string) => `${baseUrl}/instagram/verify/sendcode?chapterId=${chapterId}&code=${code}`,
  instagramTriggerVerifyCode: (chapterId: string) => `${baseUrl}/instagram/verify/trigger?chapterId=${chapterId}`
};

@Injectable({
  providedIn: 'root'
})
export class SocialMediaAdminService {

  constructor(private http: HttpClient) { }

  instagramLogin(chapterId: string): Observable<ServiceResult<void>> {
    return this.http.post(endpoints.instagramLogin(chapterId), null).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  instagramSendVerifyCode(chapterId: string, code: string): Observable<ServiceResult<void>> {
    return this.http.post(endpoints.instagramSendVerifyCode(chapterId, code), null).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  instagramTriggerVerifyCode(chapterId: string): Observable<ServiceResult<void>> {
    return this.http.post(endpoints.instagramTriggerVerifyCode(chapterId), null).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }
}
