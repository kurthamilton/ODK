import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { AdminListEventViewModel } from 'src/app/modules/admin/components/events/events/admin-list-event.view-model';

@Component({
  selector: 'app-event-list',
  templateUrl: './event-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventListComponent {
  @Input() viewModels: AdminListEventViewModel[];
}
