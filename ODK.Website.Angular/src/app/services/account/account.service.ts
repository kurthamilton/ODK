import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { MemberProperty } from 'src/app/core/members/member-property';
import { MemberSubscription } from 'src/app/core/members/member-subscription';

const baseUrl = `${environment.baseUrl}/account`;

const endpoints = {
  image: `${baseUrl}/image`,
  profile: `${baseUrl}/profile`,
  purchaseSubscription: (id: string) => `${baseUrl}/subscriptions/${id}/purchase`,
  rotateImage: `${baseUrl}/image/rotate/right`,
  subscription: `${baseUrl}/subscription`
};

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  constructor(private http: HttpClient) { }

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

  rotateImage(): Observable<string> {
    return HttpUtils.putBase64(this.http, endpoints.rotateImage);
  }

  updateImage(file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return HttpUtils.putBase64(this.http, endpoints.image, formData);
  }

  updateProfile(profile: AccountProfile): Observable<boolean> {
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

    const params: HttpParams = HttpUtils.createFormParams(paramsObject);

    return this.http.put(endpoints.profile, params).pipe(
      map(() => true)
    );
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
      type: response.type
    };
  }
}
