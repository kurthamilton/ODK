import { Event } from 'src/app/core/events/event';
import { Venue } from 'src/app/core/venues/venue';

export interface ListEventViewModel {
  event: Event;
  venue: Venue;
}