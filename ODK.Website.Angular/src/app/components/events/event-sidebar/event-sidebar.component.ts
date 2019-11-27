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

  going: Member[];
  maybe: Member[];
  memberResponse: EventResponseType;
  notGoing: Member[];
  
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

    const memberResponse: EventMemberResponse = this.responses.find(x => x.memberId === this.memberId);
    if (memberResponse) {
      this.memberResponse = memberResponse.responseType;
    }

    const responseMap: Map<string, EventMemberResponse> = ArrayUtils.toMap(this.responses, x => x.memberId);
    this.going = this.members
      .filter(x => responseMap.has(x.id) && responseMap.get(x.id).responseType === EventResponseType.Yes);

    this.maybe = this.members
      .filter(x => responseMap.has(x.id) && responseMap.get(x.id).responseType === EventResponseType.Maybe);
    
    this.notGoing = this.members
      .filter(x => responseMap.has(x.id) && responseMap.get(x.id).responseType === EventResponseType.No);

    this.changeDetector.detectChanges();
  }
}
