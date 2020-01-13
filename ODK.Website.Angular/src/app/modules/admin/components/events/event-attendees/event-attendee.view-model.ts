import { EventResponseType } from 'src/app/core/events/event-response-type';
import { Member } from 'src/app/core/members/member';

export interface EventAttendeeViewModel {
  member: Member;
  response: EventResponseType;
}