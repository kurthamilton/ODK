import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap, tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from '../../../services/events/event.service';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private eventService: EventService
  ) { 
  }

  links: {
    createEvent: string;
  };

  events: Event[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.eventService.getEvents(chapter.id))
    ).subscribe((events: Event[]) => {
      this.events = events;
      this.links = {
        createEvent: adminUrls.eventCreate(this.chapter)
      };
      this.changeDetector.detectChanges();
    });
  }

  getEventLink(event: Event): string {
    return adminUrls.event(this.chapter, event);
  }
}
