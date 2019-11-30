import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from 'src/environments/environment';
import { Member } from 'src/app/core/members/member';
import { MemberService } from './member.service';

const baseUrl: string = `${environment.baseUrl}/admin/members`;

const endpoints = {
  members: (chapterId: string) => `${baseUrl}?chapterId=${chapterId}`,
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

  updateImage(memberId: string, file: File): Observable<boolean> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.put(endpoints.updateImage(memberId), formData)
      .pipe(
        map(() => true)
      );
  }
}
