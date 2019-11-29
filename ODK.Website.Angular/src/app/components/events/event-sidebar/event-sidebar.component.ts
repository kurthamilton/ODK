import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { EventService } from 'src/app/services/events/event.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-event-sidebar',
  templateUrl: './event-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventSidebarComponent implements OnInit, OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private eventService: EventService,
    private memberService: MemberService
  ) { 
  }

  @Input() eventId: string;

  declined: Member[];
  going: Member[];
  maybe: Member[];
  memberResponse: EventResponseType;
  
  private memberId: string;
  private members: Member[];
  private responses: EventMemberResponse[];

  ngOnInit(): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    if (!token) {
      return;
    }
  
    this.memberId = token.memberId;

    this.memberService.getMembers(token.chapterId).subscribe((members: Member[]) => {
      this.members = members;
      this.setResponses();
    });

    this.ngOnChanges();
  }

  ngOnChanges(): void {
    if (!this.eventId) {
      return;
    }

    const token: AuthenticationToken = this.authenticationService.getToken();
    if (!token) {
      return;
    }        
    
    this.loadResponses(this.eventId).subscribe(() => {
      this.setResponses();
    });
  }

  onRespond(type: EventResponseType): void {
    this.eventService.respond(this.eventId, type).pipe(
      tap((response: EventMemberResponse) => this.memberResponse = response.responseType),
      switchMap(() => this.loadResponses(this.eventId))
    ).subscribe(() => {
      this.setResponses();
    });
  }

  private loadResponses(eventId: string): Observable<{}> {
    return this.eventService.getEventResponses(eventId).pipe(
      tap((responses: EventMemberResponse[]) => this.responses = responses)
    );
  }

  private setResponses(): void {
    if (!this.members || !this.responses) {
      return;
    }

    this.declined = [];
    this.going = [];
    this.maybe = [];

    const responseGroups: Map<EventResponseType, Member[]> = new Map<EventResponseType, Member[]>();
    responseGroups.set(EventResponseType.Yes, this.going);
    responseGroups.set(EventResponseType.Maybe, this.maybe);
    responseGroups.set(EventResponseType.No, this.declined);

    const memberMap: Map<string, Member> = ArrayUtils.toMap(this.members, x => x.id);
    this.responses.forEach((response: EventMemberResponse) => {
      const member: Member = memberMap.get(response.memberId);
      if (member) {
        responseGroups.get(response.responseType).push(member);
        if (response.memberId === this.memberId) {
          this.memberResponse = response.responseType;
        }
      }
    });
    
    this.changeDetector.detectChanges();
  }
}
