import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { catchApiError } from '../http/catchApiError';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { MemberProperty } from 'src/app/core/members/member-property';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { ServiceResult } from '../service-result';

const baseUrl = `${environment.apiBaseUrl}/account`;

const endpoints = {
  activate: `${baseUrl}/activate`,
  account: baseUrl,
  confirmEmailAddressUpdate: (token: string) => `${baseUrl}/profile/emailaddress/confirm?token=${encodeURIComponent(token)}`,
  emailOptIn: `${baseUrl}/emails/optin`,
  image: `${baseUrl}/image`,
  profile: `${baseUrl}/profile`,
  purchaseSubscription: (id: string) => `${baseUrl}/subscriptions/${id}/purchase`,
  register: `${baseUrl}/register`,
  requestEmailAddressUpdate: `${baseUrl}/profile/emailaddress/request`,
  rotateImage: `${baseUrl}/image/rotate/right`,
  subscription: `${baseUrl}/subscription`
};

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient) { }

  activateAccount(password: string, token: string): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      password,
      activationToken: token
    });

    return this.http.post(endpoints.activate, params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  confirmEmailAddressUpdate(token: string): Observable<ServiceResult<void>> {
    return this.http.post(endpoints.confirmEmailAddressUpdate(token), null).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  deleteAccount(): Observable<void> {
    return this.http.delete(endpoints.account).pipe(
      map(() => undefined)
    );
  }

  getProfile(): Observable<AccountProfile> {
    return this.http.get(endpoints.profile).pipe(
      map((response: any) => this.mapAccountProfile(response))
    );
  }

  getSubscription(): Observable<MemberSubscription> {
    return this.http.get(endpoints.subscription).pipe(
      map((response: any) => this.mapMemberSubscription(response))
    );
  }

  purchaseSubscription(chapterSubscriptionId: string, token: string): Observable<MemberSubscription> {
    const params: HttpParams = HttpUtils.createFormParams({
      token
    });

    return this.http.post(endpoints.purchaseSubscription(chapterSubscriptionId), params).pipe(
      map((response: any) => this.mapMemberSubscription(response))
    );
  }

  register(chapterId: string, profile: AccountProfile, image: File): Observable<ServiceResult<void>> {
    const paramsObject: any = this.createProfileParams(profile);
    paramsObject.chapterId = chapterId;
    paramsObject.image = image;

    const formData: FormData = HttpUtils.createFormData(paramsObject);

    return this.http.post(endpoints.register, formData).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  requestEmailAddressUpdate(newEmailAddress: string): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      newEmailAddress
    });

    return this.http.post(endpoints.requestEmailAddressUpdate, params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  rotateImage(): Observable<string> {
    return HttpUtils.putBase64(this.http, endpoints.rotateImage);
  }

  updateEmailOptIn(optIn: boolean): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      optIn: optIn ? 'True' : 'False'
    });

    return this.http.put(endpoints.emailOptIn, params).pipe(
      map(() => undefined)
    );
  }

  updateImage(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return HttpUtils.putBase64(this.http, endpoints.image, formData);
  }

  updateProfile(profile: AccountProfile): Observable<boolean> {
    const paramsObject = this.createProfileParams(profile);

    const params: HttpParams = HttpUtils.createFormParams(paramsObject);

    return this.http.put(endpoints.profile, params).pipe(
      map(() => true)
    );
  }

  private createProfileParams(profile: AccountProfile): {} {
    const paramsObject = {
      emailAddress: profile.emailAddress,
      emailOptIn: profile.emailOptIn ? 'true' : 'false',
      firstName: profile.firstName,
      lastName: profile.lastName
    };

    profile.properties.forEach((property: MemberProperty, i: number) => {
      paramsObject[`properties[${i}].ChapterPropertyId`] = property.chapterPropertyId;
      paramsObject[`properties[${i}].Value`] = property.value;
    });

    return paramsObject;
  }

  private mapAccountProfile(response: any): AccountProfile {
    return {
      emailAddress: response.emailAddress,
      emailOptIn: response.emailOptIn,
      firstName: response.firstName,
      joined: new Date(response.joined),
      lastName: response.lastName,
      properties: response.properties.map(x => this.mapMemberProperty(x))
    };
  }

  private mapMemberProperty(response: any): MemberProperty {
    return {
      chapterPropertyId: response.chapterPropertyId,
      value: response.value
    };
  }

  private mapMemberSubscription(response: any): MemberSubscription {
    return {
      expiryDate: response.expiryDate ? new Date(response.expiryDate) : null,
      memberId: response.memberId,
      type: response.type
    };
  }
}
