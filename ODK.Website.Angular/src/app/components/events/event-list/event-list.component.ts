import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';

import { ListEventViewModel } from '../list-event/list-event.view-model';

@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventListComponent {
  @Input() viewModels: ListEventViewModel[];
}
