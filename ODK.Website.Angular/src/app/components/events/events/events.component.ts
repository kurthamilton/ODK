import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin, of } from 'rxjs';
import { tap } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventService } from 'src/app/services/events/event.service';
import { ListEventViewModel } from '../list-event/list-event.view-model';
import { Venue } from 'src/app/core/venues/venue';
import { VenueService } from 'src/app/services/venues/venue.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private eventService: EventService,
    private venueService: VenueService,
    private authenticationService: AuthenticationService
  ) {
  }

  authenticated: boolean;
  links: {
    login: string;
    loginQueryParams: {};
  };
  viewModels: ListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private memberResponses: EventMemberResponse[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      login: appUrls.login(this.chapter),
      loginQueryParams: {
        returnUrl: appUrls.events(this.chapter)
      }
    };

    this.loadEvents(true);
  }

  private loadEvents(reloadIfError: boolean): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.authenticated = !!token;

    forkJoin([
      this.eventService.getEvents(this.chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      token && !token.membershipDisabled ? this.eventService.getMemberResponses().pipe(
        tap((responses: EventMemberResponse[]) => this.memberResponses = responses)
      ) : of(true),
      this.venueService.getVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      const venueMap: Map<string, Venue> = ArrayUtils.toMap(this.venues, x => x.id);
      const responseMap: Map<string, EventMemberResponse> = ArrayUtils.toMap(this.memberResponses || [], x => x.eventId);
      
      this.viewModels = this.events.map((event: Event): ListEventViewModel => ({
        event: event,
        memberResponse: responseMap.get(event.id),
        venue: venueMap.get(event.venueId)
      }));

      if (this.viewModels.some(x => !x.event || !x.venue)) {
       this.loadEvents(false);
       return;
      }

      this.changeDetector.detectChanges();
    });
  }
}
