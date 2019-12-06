import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { of } from 'rxjs';
import { tap, switchMap } from 'rxjs/operators';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-event',
  templateUrl: './event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  chapter: Chapter;
  event: Event;
  venue: Venue;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.eventAdminService.getEvent(id).pipe(
      tap((event: Event) => this.event = event),
      switchMap((event: Event) => event ? this.venueAdminService.getVenue(event.venueId) : of(null))
    ).subscribe((venue: Venue) => {
      if (!this.event) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
        return;
      }

      this.venue = venue;
      this.changeDetector.detectChanges();
    });
  }
}
