import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { HttpUtils } from '../http/http-utils';
import { Member } from 'src/app/core/members/member';
import { MemberService } from './member.service';

const baseUrl: string = `${environment.baseUrl}/admin/members`;

const endpoints = {
  members: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
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

  getAdminMembers(chapterId: string): Observable<Member[]> {
    return this.http.get(endpoints.members(chapterId)).pipe(
      map((response: any) => response.map(x => this.mapMember(x)))
    );
  }

  rotateMemberImage(memberId: string): Observable<string> {
    return HttpUtils.putBase64(this.http, endpoints.rotateImage(memberId));
  }

  updateImage(memberId: string, file: File): Observable<string> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return HttpUtils.putBase64(this.http, endpoints.rotateImage(memberId), formData);
  }
}
