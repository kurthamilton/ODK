import { Component, ChangeDetectionStrategy, Input, OnDestroy, OnChanges, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventInvites } from 'src/app/core/events/event-invites';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-edit-event',
  templateUrl: './edit-event.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditEventComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private eventAdminService: EventAdminService
  ) {
  }

  @Input() chapter: Chapter;
  @Input() event: Event;

  canDelete: boolean;
  formCallback: Subject<string[]> = new Subject<string[]>();

  ngOnChanges(): void {
    this.canDelete = false;

    if (!this.event) {
      return;
    }

    this.eventAdminService.getEventInvites(this.event.id).subscribe((invites: EventInvites) => {
      this.canDelete = invites.sent === 0;
      this.changeDetector.detectChanges();
    });
  }
  
  ngOnDestroy(): void {
    this.formCallback.complete();
  }  

  onDelete(): void {
    if (!confirm('Are you sure you want to delete this event?')) {
      return;
    }

    this.eventAdminService.deleteEvent(this.event.id).subscribe(() => {
      this.router.navigateByUrl(adminUrls.events(this.chapter));
    });
  }

  onFormSubmit(event: Event): void {
    this.eventAdminService.updateEvent(event).subscribe((result: ServiceResult<Event>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.events(this.chapter));
      }
    });
  }
}
