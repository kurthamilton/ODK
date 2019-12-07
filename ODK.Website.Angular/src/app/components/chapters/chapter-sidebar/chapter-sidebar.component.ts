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

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private authenticationService: AuthenticationService,
    private eventService: EventService,
    private venueService: VenueService
  ) {     
  }

  eventViewModels: ListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    
    const token: AuthenticationToken = this.authenticationService.getToken();
    if (token) {
      this.loadMemberPage();
    } else {
      this.loadPublicPage();
    }
  }

  getEventLink(event: Event): string {
    return appUrls.event(this.chapter, event);
  }

  private loadMemberPage(): void {
    forkJoin([
      this.eventService.getEvents(this.chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.venueService.getVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      this.setEvents();
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
    this.eventViewModels = this.events.map((event: Event): ListEventViewModel => ({
      event: event,
      venue: venueMap.get(event.venueId)
    }));
  }
}
