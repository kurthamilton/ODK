import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DateUtils } from 'src/app/utils/date-utils';
import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { Member } from 'src/app/core/members/member';
import { MemberService } from './member.service';
import { MemberSubscription } from 'src/app/core/members/member-subscription';

const baseUrl: string = `${environment.apiBaseUrl}/admin/members`;

const endpoints = {
  member: (memberId: string) => `${baseUrl}/${memberId}`,
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

  getAdminMembers(chapterId: string): Observable<Member[]> {
    return this.http.get(endpoints.members(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMember(x)))
    );
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

  updateMemberSubscription(subscription: MemberSubscription): Observable<void> {
    const params: HttpParams = HttpUtils.createFormParams({
      expiryDate: DateUtils.toISODateString(subscription.expiryDate),
      type: subscription.type.toString()
    });

    return this.http.put(endpoints.memberSubscription(subscription.memberId), params).pipe(
      map(() => undefined)
    );
  }

  private mapMemberSubscription(response: any): MemberSubscription {
    return {
      expiryDate: response.expiryDate ? new Date(response.expiryDate) : null,
      memberId: response.memberId,
      type: response.type
    };
  }
}
