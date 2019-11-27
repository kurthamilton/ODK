import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { switchMap, tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { Event } from 'src/app/core/events/event';
import { EventInvites } from 'src/app/core/events/event-invites';
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
  private invites: EventInvites[];
  private responses: EventMemberResponse[];

  ngOnInit(): void {
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.loadEvents(chapter))
    ).subscribe(() => {
      const eventInvitesMap: Map<string, EventInvites> = ArrayUtils.toMap(this.invites, x => x.eventId);
      const eventResponseMap: Map<string, EventMemberResponse[]> = ArrayUtils.groupValues(this.responses, x => x.eventId, x => x);

      this.viewModels = this.events.map((event: Event): ListEventViewModel => {        
        const eventInvites = eventInvitesMap.has(event.id) ? eventInvitesMap.get(event.id) : null;
        const eventResponses: EventMemberResponse[] = eventResponseMap.get(event.id) || [];
        const responseTypeMap: Map<EventResponseType, EventMemberResponse[]> = ArrayUtils.groupValues(eventResponses, x => x.responseType, x => x);
        return {
          declined: responseTypeMap.has(EventResponseType.No) ? responseTypeMap.get(EventResponseType.No).length : 0,
          event,
          going: responseTypeMap.has(EventResponseType.Yes) ? responseTypeMap.get(EventResponseType.Yes).length : 0,
          invitesFailed: eventInvites ? eventInvites.sent - eventInvites.delivered : 0,
          invitesSent: eventInvites ? eventInvites.sent : 0,
          maybe: responseTypeMap.has(EventResponseType.Maybe) ? responseTypeMap.get(EventResponseType.Maybe).length : 0,
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

  private loadEvents(chapter: Chapter): Observable<{}> {
    return forkJoin([
      this.eventService.getEvents(chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.eventService.getChapterInvites(chapter.id).pipe(
        tap((invites: EventInvites[]) => this.invites = invites)
      ),
      this.eventService.getChapterResponses(chapter.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      )
    ]);
  }
}
