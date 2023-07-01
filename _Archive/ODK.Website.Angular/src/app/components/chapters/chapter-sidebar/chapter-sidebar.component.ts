import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
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
import { ListEventViewModel } from '../../events/list-event/list-event.view-model';
import { Venue } from 'src/app/core/venues/venue';
import { VenueService } from 'src/app/services/venues/venue.service';

@Component({
  selector: 'app-chapter-sidebar',
  templateUrl: './chapter-sidebar.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSidebarComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private authenticationService: AuthenticationService,
    private eventService: EventService,
    private venueService: VenueService
  ) {
  }

  eventViewModels: ListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private memberResponses: EventMemberResponse[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

    const token: AuthenticationToken = this.authenticationService.getToken();
    if (token) {
      this.loadMemberPage(true);
    } else {
      this.loadPublicPage();
    }
  }

  getEventLink(event: Event): string {
    return appUrls.event(this.chapter, event);
  }

  private loadMemberPage(reloadIfError: boolean): void {
    forkJoin([
      this.eventService.getEvents(this.chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.eventService.getMemberResponses().pipe(
        tap((responses: EventMemberResponse[]) => this.memberResponses = responses)
      ),
      this.venueService.getVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      this.setEvents();

      if (this.eventViewModels.some(x => !x.event || !x.venue) && reloadIfError) {
        this.loadMemberPage(false);
        return;
      }
      this.changeDetector.detectChanges();
    });
  }

  private loadPublicPage(): void {
    forkJoin([
      this.eventService.getPublicEvents(this.chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.venueService.getPublicVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      this.setEvents();
      this.changeDetector.detectChanges();
    });
  }

  private setEvents(): void {
    const venueMap: Map<string, Venue> = ArrayUtils.toMap(this.venues, x => x.id);
    const responseMap: Map<string, EventMemberResponse> = ArrayUtils.toMap(this.memberResponses || [], x => x.eventId);

    this.eventViewModels = this.events.map((event: Event): ListEventViewModel => ({
      event,
      memberResponse: responseMap.get(event.id),
      venue: venueMap.get(event.venueId)
    }));
  }
}
