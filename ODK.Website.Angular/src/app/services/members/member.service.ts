import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { Member } from 'src/app/core/members/member';
import { MemberProfile } from 'src/app/core/members/member-profile';
import { MemberProperty } from 'src/app/core/members/member-property';

const baseUrl = `${environment.baseUrl}/members`;

const endpoints = {
  memberImage: (memberId: string) => `${baseUrl}/${memberId}/image`,
  memberProfile: (memberId: string) => `${baseUrl}/${memberId}/profile`,
  members: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`
}

@Injectable({
  providedIn: 'root'
})
export class MemberService {

  constructor(private http: HttpClient) { }

  getMember(id: string, chapterId: string): Observable<Member> {
    return this.getMembers(chapterId).pipe(
      map((members: Member[]) => members.find(x => x.id === id))
    );
  }

  getMemberImage(id: string): Observable<string> {
    return HttpUtils.getBase64(this.http, endpoints.memberImage(id));
  }

  getMemberProfile(id: string): Observable<MemberProfile> {
    return this.http.get(endpoints.memberProfile(id)).pipe(
      map((response: any) => this.mapMemberProfile(response))
    );
  }

  getMembers(chapterId: string): Observable<Member[]> {
    return this.http.get(endpoints.members(chapterId))
      .pipe(
        map((response: any) => response.map(x => this.mapMember(x)))
      );
  }

  private mapMember(response: any): Member {
    return {
      chapterId: response.chapterId,
      firstName: response.firstName,
      fullName: `${response.firstName} ${response.lastName}`,
      id: response.id,
      imageUrl: response.imageUrl,
      lastName: response.lastName
    };
  }

  private mapMemberProfile(response: any): MemberProfile {
    return {
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
}
