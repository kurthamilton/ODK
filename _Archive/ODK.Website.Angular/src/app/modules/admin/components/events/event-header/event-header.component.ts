import { Component, Input, ChangeDetectionStrategy, OnChanges, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-event-header',
  templateUrl: './event-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventHeaderComponent implements OnChanges {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  @Input() event: Event;

  links: {
    live: string;
    venue: string;
  };
  venue: Venue;

  private chapter: Chapter;

  ngOnChanges(): void {
    if (!this.event) {
      return;
    }

    this.chapter = this.chapterAdminService.getActiveChapter();

    this.venueAdminService.getVenue(this.event.venueId).subscribe((venue: Venue) => {
      this.venue = venue;
      this.links = {
        live: appUrls.event(this.chapter, this.event),
        venue: adminUrls.venue(this.chapter, venue)
      };
      this.changeDetector.detectChanges();
    });
  }

}
