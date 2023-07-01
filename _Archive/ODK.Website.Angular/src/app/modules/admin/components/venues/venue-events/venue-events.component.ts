import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-venue-events',
  templateUrl: './venue-events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenueEventsComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  events: Event[];
  links: {
    createEvent: string;
    createEventParams: {}
  };
  venue: Venue;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.venue = this.venueAdminService.getActiveVenue();
    this.links = {
      createEvent: adminUrls.eventCreate(this.chapter),
      createEventParams: {
        venue: this.venue.id
      }
    };

    this.eventAdminService.getEventsByVenue(this.venue.id).subscribe((events: Event[]) => {
      this.events = events.sort((a, b) => DateUtils.compare(b.date, a.date));
      this.changeDetector.detectChanges();
    });
  }

  getEventLink(event: Event): string {
    return adminUrls.event(this.chapter, event);
  }
}
