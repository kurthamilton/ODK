import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { MemberService } from 'src/app/services/members/member.service';
import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-event-responses',
  templateUrl: './event-responses.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventResponsesComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private memberService: MemberService
  ) {     
  }

  @Input() eventId: string;

  declined: Member[];
  going: Member[];
  maybe: Member[];
  responses: EventMemberResponse[];
  
  private members: Member[];

  ngOnChanges(): void {
    if (!this.eventId) {
      return;
    }

    const chapter: Chapter = this.chapterAdminService.getActiveChapter();

    forkJoin(
      this.memberService.getMembers(chapter.id).pipe(
        tap((members: Member[]) => this.members = members)
      ),
      this.eventAdminService.getEventResponses(this.eventId).pipe(
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
