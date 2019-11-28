import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventService } from 'src/app/services/events/event.service';

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

  chapter: Chapter;
  events: Event[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.eventService.getEvents(this.chapter.id).subscribe((events: Event[]) => {
      this.chapter = this.chapter;
      this.events = events;
      this.changeDetector.detectChanges();
    });
  }
}
