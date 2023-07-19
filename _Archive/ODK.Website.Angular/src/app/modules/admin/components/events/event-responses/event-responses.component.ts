import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { Member } from 'src/app/core/members/member';
import { MemberResponseGroupViewModel } from './member-response-group.view-model';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-event-responses',
  templateUrl: './event-responses.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventResponsesComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private memberService: MemberService
  ) {
  }

  chapter: Chapter;
  event: Event;
  memberGroups: MemberResponseGroupViewModel[];
  responses: EventMemberResponse[];

  private members: Member[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.event = this.eventAdminService.getActiveEvent();

    forkJoin([
      this.memberService.getMembers(this.chapter.id).pipe(
        tap((members: Member[]) => this.members = members)
      ),
      this.eventAdminService.getEventResponses(this.event.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      )
    ]).subscribe(() => {
      const memberMap: Map<string, Member> = ArrayUtils.toMap(this.members, x => x.id);

      this.memberGroups = [];

      const responseMap: Map<EventResponseType, MemberResponseGroupViewModel> =
        new Map<EventResponseType, MemberResponseGroupViewModel>();
      responseMap.set(EventResponseType.Yes, this.memberGroups[this.memberGroups.push({
        members: [],
        title: 'Going'
      }) - 1]);
      responseMap.set(EventResponseType.Maybe, this.memberGroups[this.memberGroups.push({
        members: [],
        title: 'Maybe'
      }) - 1]);
      responseMap.set(EventResponseType.No, this.memberGroups[this.memberGroups.push({
        members: [],
        title: 'Declined'
      }) - 1]);
      responseMap.set(EventResponseType.None, this.memberGroups[this.memberGroups.push({
        members: [],
        title: 'No response'
      }) - 1]);
      responseMap.set(EventResponseType.NotInvited, this.memberGroups[this.memberGroups.push({
        members: [],
        title: 'Not invited'
      }) - 1]);

      this.responses
        .sort((a, b) => memberMap.get(a.memberId).fullName.localeCompare(memberMap.get(b.memberId).fullName))
        .forEach((response: EventMemberResponse) => {
          const member: Member = memberMap.get(response.memberId);
          if (member) {
            if (responseMap.has(response.responseType)) {
              responseMap.get(response.responseType).members.push(member);
            }
          }
        });

      this.changeDetector.detectChanges();
    });
  }
}
