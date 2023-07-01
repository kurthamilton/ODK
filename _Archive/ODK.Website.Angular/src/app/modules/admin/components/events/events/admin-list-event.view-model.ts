import { Event } from 'src/app/core/events/event';
import { Venue } from 'src/app/core/venues/venue';

export interface AdminListEventViewModel {
  canSendInvites: boolean;
  declined: number;
  event: Event;
  going: number;
  invitesSent: number;
  maybe: number;
  venue: Venue;
}
