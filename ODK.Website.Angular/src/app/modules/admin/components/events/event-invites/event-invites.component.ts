import { Component, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnChanges } from '@angular/core';

import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventInvites } from 'src/app/core/events/event-invites';

@Component({
  selector: 'app-event-invites',
  templateUrl: './event-invites.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventInvitesComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService
  ) {     
  }

  @Input() eventId: string;

  invites: EventInvites;
  sending = false;
  sent = false;

  ngOnChanges(): void {
    if (!this.eventId) {
      return;
    }
    
    this.loadStatistics();
  }

  onSend(): void {
    this.sending = true;
    this.changeDetector.detectChanges();

    this.eventAdminService.sendInvites(this.eventId).subscribe(() => {
      this.sending = false;
      this.sent = true;
      this.loadStatistics();
    });
  }

  private loadStatistics(): void {
    this.eventAdminService.getEventInvites(this.eventId).subscribe((invites: EventInvites) => {
      this.invites = invites;
      this.changeDetector.detectChanges();
    });
  }
}
