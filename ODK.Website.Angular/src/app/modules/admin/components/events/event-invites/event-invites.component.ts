import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventInvites } from 'src/app/core/events/event-invites';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';

@Component({
  selector: 'app-event-invites',
  templateUrl: './event-invites.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventInvitesComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private eventAdminService: EventAdminService
  ) {     
  }

  event: Event;
  invites: EventInvites;
  sendForm: FormViewModel;
  sendTestForm: FormViewModel;

  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    const eventId: string = this.route.snapshot.paramMap.get(adminPaths.events.event.params.id);
    this.eventAdminService.getEvent(eventId).subscribe((event: Event) => {
      this.event = event;
      this.loadStatistics();
    });    
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onSendFormSubmit(): void {
    if (!confirm('Are you sure you want to send invite emails for this event?')) {
      this.formCallback.next(true);
      return;
    }

    this.sendEmails();
  }

  onSendTestFormSubmit(): void {
    if (!confirm('Are you sure you want to send a test invite email to yourself?')) {
      this.formCallback.next(true);
      return;
    }

    this.sendEmails(true);
  }

  private buildForms(): void {
    if (this.invites.sent > 0) {
      this.sendForm = null;
      this.sendTestForm = null;
      return;
    }

    this.sendForm = {
      buttons: [
        {  
          text: 'Send invites',
          type: 'success'
        }
      ],
      callback: this.formCallback,
      controls: []
    };

    this.sendTestForm = {
      buttons: [
        {  
          text: 'Send test',
          type: 'primary'
        }
      ],
      callback: this.formCallback,
      controls: []
    };
  }

  private loadStatistics(): void {
    this.eventAdminService.getEventInvites(this.event.id).subscribe((invites: EventInvites) => {
      this.invites = invites;
      this.buildForms();
      this.changeDetector.detectChanges();
    });
  }

  private sendEmails(test?: boolean): void {
    this.changeDetector.detectChanges();

    this.eventAdminService.sendInvites(this.event.id, test).subscribe(() => {
      this.formCallback.next(true);
      this.loadStatistics();
    });
  }
}
