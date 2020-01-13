import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventAttendeeViewModel } from './event-attendee.view-model';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { LoadingSpinnerOptions } from 'src/app/modules/shared/components/elements/loading-spinner/loading-spinner-options';
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
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true
  };

  private event: Event;
  private members: Member[];
  private responseMap: Map<string, EventResponseType>;

  ngOnInit(): void {
    this.event = this.eventAdminService.getActiveEvent();

    forkJoin([
      this.eventAdminService.getEventResponses(this.event.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responseMap = ArrayUtils.toValueMap(responses, x => x.memberId, x => x.responseType))
      ),
      this.memberAdminService.getMembers(this.event.chapterId).pipe(
        tap((members: Member[]) => this.members = members.sort((a, b) => a.fullName.localeCompare(b.fullName)))
      )
    ]).subscribe(() => {
      this.setAttendees();
      this.changeDetector.detectChanges();
    });
  }

  onRespond(member: Member, responseType: EventResponseType): void {
    const viewModel: EventAttendeeViewModel = this.attendees.find(x => x.member.id === member.id);
    viewModel.updating = true;
    this.changeDetector.detectChanges();

    this.eventAdminService.updateMemberResponse(this.event.id, member.id, responseType).subscribe((response: EventMemberResponse) => {      
      viewModel.response = response.responseType;
      viewModel.updating = false;
      this.changeDetector.detectChanges();
    });
  }

  private setAttendees(): void {
    this.attendees = this.members.map((x: Member): EventAttendeeViewModel => ({
      member: x,
      response: this.responseMap.get(x.id) ? this.responseMap.get(x.id) : null
    }));
  }
}
