import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateUtils } from 'src/app/utils/date-utils';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MemberEventViewModel } from './member-event.view-model';
import { Venue } from 'src/app/core/venues/venue';
import { VenueAdminService } from 'src/app/services/venues/venue-admin.service';

@Component({
  selector: 'app-member-events',
  templateUrl: './member-events.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberEventsComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private memberAdminService: MemberAdminService,
    private eventAdminService: EventAdminService,
    private venueAdminService: VenueAdminService,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  viewModels: MemberEventViewModel[];

  private chapter: Chapter;
  private events: Event[];
  private member: Member;
  private responses: EventMemberResponse[];
  private venues: Venue[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.member = this.memberAdminService.getActiveMember();

    forkJoin([
      this.eventAdminService.getAllEvents(this.member.chapterId, 1, 100).pipe(
        tap((events: Event[]) => this.events = events)
      ),
      this.eventAdminService.getAllMemberResponses(this.member.id).pipe(
        tap((responses: EventMemberResponse[]) => this.responses = responses)
      ),
      this.venueAdminService.getAdminVenues(this.member.chapterId).pipe(
        tap((venues: Venue[]) => this.venues = venues)
      )
    ]).subscribe(() => {
      const responseMap: Map<string, EventMemberResponse> = ArrayUtils.toMap(this.responses, x => x.eventId);
      const venueMap: Map<string, Venue> = ArrayUtils.toMap(this.venues, x => x.id);

      this.viewModels = this.events
        .filter(x => responseMap.has(x.id))
        .sort((a, b) => DateUtils.compare(b.date, a.date))
        .map((event: Event): MemberEventViewModel => ({
          event,
          response: responseMap.get(event.id),
          venue: venueMap.get(event.venueId)
        }));
      this.changeDetector.detectChanges();
    });
  }

  getEventLink(event: Event): string {
    return adminUrls.event(this.chapter, event);
  }

  getVenueLink(venue: Venue): string {
    return adminUrls.venue(this.chapter, venue);
  }
}
