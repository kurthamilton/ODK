import { EventResponseType } from './event-response-type';

export interface EventMemberResponse {
    eventId: string;
    memberId: string;
    responseType: EventResponseType;
}
