import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-create-event',
  templateUrl: './create-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateEventComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private route: ActivatedRoute,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  event: Event;
  formCallback: Subject<string[]> = new Subject<string[]>();

  private chapter: Chapter;
  private venue: Venue;

  ngOnInit(): void {    
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.events(this.chapter), text: 'Events' }
    ];

    const venueId: string = this.route.snapshot.queryParamMap.get(adminPaths.events.create.queryParams.venue);
    if (venueId) {
      this.venueAdminService.getVenue(venueId).subscribe((venue: Venue) => {
        this.venue = venue;
        this.event = this.createEmptyEvent();
        this.changeDetector.detectChanges();
      });
    } else {
      this.event = this.createEmptyEvent();
    }
  }  

  onFormSubmit(event: Event): void {
    this.eventAdminService.createEvent(event).subscribe((result: ServiceResult<Event>) => {
      this.formCallback.next(result.messages);

      if (result.success) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
      }
    });
  }

  private createEmptyEvent(): Event {
    return {
      chapterId: this.chapter.id,
      date: new Date(),
      description: '',
      id: '',
      imageUrl: '',
      isPublic: false,
      name: '',
      time: '',
      venueId: this.venue ? this.venue.id : ''
    };
  }
}
