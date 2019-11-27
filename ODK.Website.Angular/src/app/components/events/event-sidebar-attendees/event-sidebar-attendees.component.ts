import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { Member } from 'src/app/core/members/member';

@Component({
  selector: 'app-event-sidebar-attendees',
  templateUrl: './event-sidebar-attendees.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventSidebarAttendeesComponent {

  @Input() members: Member[];
  @Input() title: string;

}
