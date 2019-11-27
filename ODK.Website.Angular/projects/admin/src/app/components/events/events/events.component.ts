import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { EventService } from '../../../services/events/event.service';
import { ListEventViewModel } from './list-event.view-model';

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

  viewModels: ListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private responses: Map<string, EventMemberResponse[]>;

  ngOnInit(): void {
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => forkJoin([
        this.eventService.getEvents(chapter.id).pipe(
          tap((events: Event[]) => this.events = events)
        ),
        this.eventService.getChapterResponses(chapter.id).pipe(
          tap((responses: EventMemberResponse[]) => this.responses = ArrayUtils.groupValues(responses, x => x.eventId, x => x))
        )
      ]))
    ).subscribe(() => {
      this.viewModels = this.events.map((event: Event): ListEventViewModel => {
        const responses: EventMemberResponse[] = this.responses[event.id] || [];
        return {
          canBeDeleted: responses.length === 0,
          event,
          going: responses.filter(x => x.responseType === EventResponseType.Yes).length
        }
      });
      
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
