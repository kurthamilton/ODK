import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AdminListEventViewModel } from './admin-list-event.view-model';
import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventInvites } from 'src/app/core/events/event-invites';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';
import { Venue } from 'src/app/core/venues/venue';

const pageSize = 10;

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
  page = 1;
  pageCount: number;
  viewModels: AdminListEventViewModel[];

  private chapter: Chapter;
  private eventInvitesMap: Map<string, EventInvites>;
  private eventResponseMap: Map<string, EventMemberResponse[]>;  
  private events: Event[];
  private invites: EventInvites[];
  private responses: EventMemberResponse[];
  private totalEventCount: number;
  private venueMap: Map<string, Venue>;
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      forkJoin([
        this.eventAdminService.getEventCount(this.chapter.id).pipe(
          tap((count: number) => this.totalEventCount = count)
        ),
        this.eventAdminService.getChapterResponses(this.chapter.id).pipe(
          tap((responses: EventMemberResponse[]) => this.responses = responses)
        ),
        this.venueAdminService.getVenues(this.chapter.id).pipe(
          tap((venues: Venue[]) => this.venues = venues)
        )
      ]),
      this.loadEvents()
    ]).subscribe(() => {
      this.eventInvitesMap = ArrayUtils.toMap(this.invites, x => x.eventId);
      this.eventResponseMap = ArrayUtils.groupValues(this.responses, x => x.eventId, x => x);
      this.venueMap = ArrayUtils.toMap(this.venues, x => x.id);

      this.links = {
        createEvent: adminUrls.eventCreate(this.chapter)
      };
      this.pageCount = Math.ceil(this.totalEventCount / pageSize);

      this.setViewModels();
      
      this.changeDetector.detectChanges();
    });
  }

  getEventInvitesLink(event: Event): string {
    return adminUrls.eventInvites(this.chapter, event);    
  }

  getEventLink(event: Event): string {
    return adminUrls.event(this.chapter, event);
  }

  getVenueLink(venue: Venue): string {
    return adminUrls.venue(this.chapter, venue);
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadEvents().subscribe(() => {
      this.setViewModels();
      this.changeDetector.detectChanges();
    });
  }

  private loadEvents(): Observable<{}> {
    this.viewModels = null;
    this.changeDetector.detectChanges();
    
    return forkJoin([
      this.eventAdminService.getAllEvents(this.chapter.id, this.page, this.pageCount).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.eventAdminService.getChapterInvites(this.chapter.id, this.page, this.pageCount).pipe(
        tap((invites: EventInvites[]) => this.invites = invites)
      ),
    ]);
  }

  private setViewModels(): void {
    const today: Date = DateUtils.today();

    this.viewModels = this.events.map((event: Event): AdminListEventViewModel => {
      const eventInvites = this.eventInvitesMap.has(event.id) ? this.eventInvitesMap.get(event.id) : null;
      const eventResponses: EventMemberResponse[] = this.eventResponseMap.get(event.id) || [];
      const responseTypeMap: Map<EventResponseType, EventMemberResponse[]> = ArrayUtils.groupValues(eventResponses, x => x.responseType, x => x);

      return {
        canSendInvites: event.date >= today && (!eventInvites || !eventInvites.sentDate),
        declined: responseTypeMap.has(EventResponseType.No) ? responseTypeMap.get(EventResponseType.No).length : 0,
        event,
        going: responseTypeMap.has(EventResponseType.Yes) ? responseTypeMap.get(EventResponseType.Yes).length : 0,
        invitesSent: !!eventInvites ? eventInvites.sent : 0,
        maybe: responseTypeMap.has(EventResponseType.Maybe) ? responseTypeMap.get(EventResponseType.Maybe).length : 0,
        venue: this.venueMap.get(event.venueId)
      };
    });
  }
}
