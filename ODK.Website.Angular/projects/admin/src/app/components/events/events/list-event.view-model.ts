import { Event } from 'src/app/core/events/event';

export interface ListEventViewModel {
    canBeDeleted: boolean;
    event: Event;
    going: number;
}
