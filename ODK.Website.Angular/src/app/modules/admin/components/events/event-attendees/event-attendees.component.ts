import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventAttendeeViewModel } from './event-attendee.view-model';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-event-attendees',
  templateUrl: './event-attendees.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventAttendeesComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService,
    private memberAdminService: MemberAdminService
  ) {     
  }

  attendees: EventAttendeeViewModel[];

  private event: Event;
  private members: Member[];
  private responses: EventMemberResponse[];

  ngOnInit(): void {
    this.event = this.eventAdminService.getActiveEvent();

    forkJoin([
      this.eventAdminService.getEventResponses(this.event.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      ),
      this.memberAdminService.getMembers(this.event.chapterId).pipe(
        tap((members: Member[]) => this.members = members.sort((a, b) => a.fullName.localeCompare(b.fullName)))
      )
    ]).subscribe(() => {
      const responseMap: Map<string, EventMemberResponse> = ArrayUtils.toMap(this.responses, x => x.memberId);
      this.attendees = this.members.map((x: Member): EventAttendeeViewModel => ({
        member: x,
        response: responseMap.get(x.id) ? responseMap.get(x.id).responseType : null
      }));
      this.changeDetector.detectChanges();
    });
  }

  onRespond(member: Member, responseType: EventResponseType): void {
    this.eventAdminService.updateMemberResponse(this.event.id, member.id, responseType).subscribe((response: EventMemberResponse) => {
      // TODO: integrate response into attendees array
      // TODO: show loading spinner while updating
    });
  }
}
