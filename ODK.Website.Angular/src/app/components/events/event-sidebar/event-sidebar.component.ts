import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Observable, forkJoin } from 'rxjs';
import { tap, switchMap, map } from 'rxjs/operators';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
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
export class EventSidebarComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private eventService: EventService,
    private memberService: MemberService
  ) { 
  }

  @Input() event: Event;

  authenticated: boolean;
  declined: Member[];
  going: Member[];
  maybe: Member[];
  memberResponse: EventResponseType;
  
  private chapter: Chapter;
  private memberId: string;
  private members: Member[];
  private responses: EventMemberResponse[];

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }    
    
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.authenticated = !!token;
    if (!token) {      
      return;
    }
  
    this.chapter = this.chapterService.getActiveChapter();
    this.memberId = token.memberId;
    
    forkJoin([
      this.memberService.getMembers(token.chapterId).pipe(
        tap((members: Member[]) => this.members = members)
      ),
      this.loadResponses()
    ]).subscribe(() => {
      this.setResponses();

      const rsvp: string = this.route.snapshot.queryParamMap.get(appPaths.chapter.childPaths.event.queryParams.rsvp);
      if (rsvp === 'yes') {
        this.onRespond(EventResponseType.Yes);
      }
      this.changeDetector.detectChanges();
    });
  }

  onRespond(type: EventResponseType): void {
    this.eventService.respond(this.event.id, type).pipe(
      tap((response: EventMemberResponse) => this.memberResponse = response.responseType),
      switchMap(() => this.loadResponses())
    ).subscribe(() => {
      this.router.navigateByUrl(appUrls.event(this.chapter, this.event));
      this.setResponses();
      this.changeDetector.detectChanges();
    });
  }

  private loadResponses(): Observable<void> {
    return this.eventService.getEventResponses(this.event.id).pipe(
      tap((responses: EventMemberResponse[]) => this.responses = responses),
      map(() => undefined)
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
    this.responses
      .sort((a, b) => memberMap.get(a.memberId).fullName.localeCompare(memberMap.get(b.memberId).fullName))
      .forEach((response: EventMemberResponse) => {
        const member: Member = memberMap.get(response.memberId);
        if (member) {
          responseGroups.get(response.responseType).push(member);
          if (response.memberId === this.memberId) {
            this.memberResponse = response.responseType;
          }
        }
      });        
  }
}
