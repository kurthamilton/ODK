import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { Event } from 'src/app/core/events/event';

@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventListComponent {
  @Input() events: Event[];
}
