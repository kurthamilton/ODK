import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { Venue } from 'src/app/core/venues/venue';

@Component({
  selector: 'app-venue-events',
  templateUrl: './venue-events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class VenueEventsComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  @Input() venue: Venue;

  events: Event[];

  private chapter: Chapter;

  ngOnChanges(): void {
    if (!this.venue) {
      return;
    }

    this.chapter = this.chapterAdminService.getActiveChapter();

    this.eventAdminService.getEventsByVenue(this.venue.id).subscribe((events: Event[]) => {
      this.events = events.sort((a, b) => DateUtils.compare(b.date, a.date));
      this.changeDetector.detectChanges();
    });
  }

  getEventLink(event: Event): string {
    return adminUrls.event(this.chapter, event);
  }
}
