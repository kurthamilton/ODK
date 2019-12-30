import { Event } from 'src/app/core/events/event';
import { EventMemberResponse } from 'src/app/core/events/event-member-response';
import { Venue } from 'src/app/core/venues/venue';

export interface ListEventViewModel {
  event: Event;
  memberResponse: EventMemberResponse;
  venue: Venue;
}