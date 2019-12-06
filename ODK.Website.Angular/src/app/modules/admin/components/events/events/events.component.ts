import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AdminListEventViewModel } from './admin-list-event.view-model';
import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventInvites } from 'src/app/core/events/event-invites';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';
import { Venue } from 'src/app/core/venues/venue';

@Component({
  selector: 'app-events',
  templateUrl: './events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private eventAdminService: EventAdminService,
    private venueAdminService: VenueAdminService
  ) {
  }

  links: {
    createEvent: string;
  };

  viewModels: AdminListEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private invites: EventInvites[];
  private responses: EventMemberResponse[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.loadEvents(this.chapter).subscribe(() => {
      const eventInvitesMap: Map<string, EventInvites> = ArrayUtils.toMap(this.invites, x => x.eventId);
      const eventResponseMap: Map<string, EventMemberResponse[]> = ArrayUtils.groupValues(this.responses, x => x.eventId, x => x);
      const venueMap: Map<string, Venue> = ArrayUtils.toMap(this.venues, x => x.id);

      this.viewModels = this.events.map((event: Event): AdminListEventViewModel => {
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
          venue: venueMap.get(event.venueId)
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

  getVenueLink(venue: Venue): string {
    return adminUrls.venue(this.chapter, venue);
  }

  private loadEvents(chapter: Chapter): Observable<{}> {
    return forkJoin([
      this.eventAdminService.getEvents(chapter.id).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.eventAdminService.getChapterInvites(chapter.id).pipe(
        tap((invites: EventInvites[]) => this.invites = invites)
      ),
      this.eventAdminService.getChapterResponses(chapter.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      ),
      this.venueAdminService.getVenues(chapter.id).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]);
  }
}
