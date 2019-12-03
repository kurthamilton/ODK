import { Event } from 'src/app/core/events/event';

export interface ListEventViewModel {
    declined: number;
    event: Event;
    going: number;
    invitesFailed: number;
    invitesSent: number;
    maybe: number;
}
