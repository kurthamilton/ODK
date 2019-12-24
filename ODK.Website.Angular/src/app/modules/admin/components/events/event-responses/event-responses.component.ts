import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-event-responses',
  templateUrl: './event-responses.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventResponsesComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private memberService: MemberService
  ) {
  }  

  declined: Member[];
  event: Event;
  going: Member[];
  maybe: Member[];
  responses: EventMemberResponse[];

  private members: Member[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const eventId: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);
    
    forkJoin(
      this.eventAdminService.getEvent(eventId).pipe(
        tap((event: Event) => this.event = event)
      ),
      this.memberService.getMembers(chapter.id).pipe(
        tap((members: Member[]) => this.members = members)
      ),
      this.eventAdminService.getEventResponses(eventId).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      )
    ).subscribe(() => {
      const memberMap: Map<string, Member> = ArrayUtils.toMap(this.members, x => x.id);

      this.going = [];
      this.maybe = [];
      this.declined = [];

      const responseMap: Map<EventResponseType, Member[]> = new Map<EventResponseType, Member[]>();
      responseMap.set(EventResponseType.Yes, this.going);
      responseMap.set(EventResponseType.Maybe, this.maybe);
      responseMap.set(EventResponseType.No, this.declined);

      this.responses.forEach((response: EventMemberResponse) => {
        const member: Member = memberMap.get(response.memberId);
        if (member) {
          if (responseMap.has(response.responseType)) {
            responseMap.get(response.responseType).push(member);
          }
        }
      });

      this.changeDetector.detectChanges();
    });
  }

}
