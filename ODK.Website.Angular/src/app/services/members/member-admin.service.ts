import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { AdminMember } from 'src/app/core/members/admin-member';
import { catchApiError } from '../http/catchApiError';
import { DateUtils } from 'src/app/utils/date-utils';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { Member } from 'src/app/core/members/member';
import { MemberEmail } from 'src/app/core/members/member-email';
import { MemberService } from './member.service';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { ServiceResult } from '../service-result';

const baseUrl: string = `${environment.adminApiBaseUrl}/members`;

const endpoints = {
  emails: (chapterId: string) => `${baseUrl}/emails?chapterId=${chapterId}`,
  member: (memberId: string) => `${baseUrl}/${memberId}`,
  memberImport: (chapterId: string) => `${baseUrl}/import?chapterId=${chapterId}`,
  memberImportTemplate: (chapterId: string) => `${baseUrl}/import/template?chapterId=${chapterId}`,
  members: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
  memberSubscription: (memberId: string) => `${baseUrl}/${memberId}/subscription`,
  memberSubscriptions: (chapterId: string) => `${baseUrl}/subscriptions?chapterId=${chapterId}`,
  rotateImage: (memberId: string) => `${baseUrl}/${memberId}/image/rotate/right`,
  updateImage: (memberId: string) => `${baseUrl}/${memberId}/image`
};

@Injectable({
  providedIn: 'root'
})
export class MemberAdminService extends MemberService {

  constructor(http: HttpClient) {
    super(http);
  }

  private activeMember: Member;

  deleteMember(memberId: string): Observable<void> {
    return this.http.delete(endpoints.member(memberId)).pipe(
      map(() => undefined)
    );
  }

  getActiveMember(): Member {
    return this.activeMember;
  }

  getAdminMember(memberId: string): Observable<Member> {
    return this.http.get(endpoints.member(memberId)).pipe(
      map((response: any) => this.mapMember(response))
    );
  }

  getAdminMembers(chapterId: string): Observable<AdminMember[]> {
    return this.http.get(endpoints.members(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapAdminMember(x)))
    );
  }

  getMemberEmails(chapterId: string): Observable<MemberEmail[]> {
    return this.http.get(endpoints.emails(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMemberEmail(x)))
    );
  }

  getMemberImportTemplateUrl(chapterId: string): string {
    return endpoints.memberImportTemplate(chapterId);
  }

  getMemberSubscription(memberId: string): Observable<MemberSubscription> {
    return this.http.get(endpoints.memberSubscription(memberId)).pipe(
      map((response: any) => this.mapMemberSubscription(response))
    );
  }

  getMemberSubscriptions(chapterId: string): Observable<MemberSubscription[]> {
    return this.http.get(endpoints.memberSubscriptions(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMemberSubscription(x)))
    );
  }

  importMembers(chapterId: string, file: File): Observable<ServiceResult<void>> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(endpoints.memberImport(chapterId), formData).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  rotateMemberImage(memberId: string): Observable<string> {
    return HttpUtils.putBase64(this.http, endpoints.rotateImage(memberId));
  }

  setActiveMember(member: Member): void {
    this.activeMember = member;
  }

  updateImage(memberId: string, file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return HttpUtils.putBase64(this.http, endpoints.updateImage(memberId), formData);
  }

  updateMemberSubscription(subscription: MemberSubscription): Observable<ServiceResult<void>> {
    const params: HttpParams = HttpUtils.createFormParams({
      expiryDate: DateUtils.toISODateString(subscription.expiryDate),
      type: subscription.type.toString()
    });

    return this.http.put(endpoints.memberSubscription(subscription.memberId), params).pipe(
      map((): ServiceResult<void> => ({
        success: true
      })),
      catchApiError()
    );
  }

  private mapAdminMember(response: any): AdminMember {
    return Object.assign({
      emailOptIn: response.emailOptIn
    }, this.mapMember(response));
  }

  private mapMemberEmail(response: any): MemberEmail {
    return {
      emailAddress: response.emailAddress,
      memberId: response.memberId
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
