import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { TitleService } from 'src/app/services/title/title.service';
import { Venue } from 'src/app/core/venues/venue';
import { VenueService } from 'src/app/services/venues/venue.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterService: ChapterService,
    private eventService: EventService,
    private titleService: TitleService,
    private venueService: VenueService
  ) {
  }

  breadcrumbs: MenuItem[];
  chapter: Chapter;
  event: Event;
  eventId: string;
  venue: Venue;

  ngOnInit(): void {
    this.eventId = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.event.params.id);

    this.chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: appUrls.events(this.chapter), text: 'Events' }
    ];

    this.eventService.getEvent(this.eventId, this.chapter.id).pipe(
      tap((event: Event) => this.event = event),
      switchMap((event: Event) => event ? this.venueService.getVenue(event.venueId) : of(null))
    ).subscribe((venue: Venue) => {
      if (!this.event) {
        this.router.navigateByUrl(appUrls.events(this.chapter));
        return;
      }

      this.venue = venue;
      this.titleService.setRouteTitle(this.event.name, 'Events');
      this.changeDetector.detectChanges();
    });
  }
}
