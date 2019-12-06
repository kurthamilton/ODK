import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
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
    private venueService: VenueService
  ) {
  }

  viewModels: ListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

    forkJoin([
      this.eventService.getEvents(this.chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.venueService.getVenues(this.chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      const venueMap: Map<string, Venue> = ArrayUtils.toMap(this.venues, x => x.id);

      this.viewModels = this.events.map((event: Event): ListEventViewModel => ({
        event: event,
        venue: venueMap.get(event.venueId)
      }));

      this.changeDetector.detectChanges();
    });
  }
}
