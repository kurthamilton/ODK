import { Event } from 'src/app/core/events/event';

export interface ListEventViewModel {
    canBeDeleted: boolean;
    declined: number;
    event: Event;
    going: number;
    maybe: number;
}
