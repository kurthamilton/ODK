import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

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
    private eventAdminService: EventAdminService
  ) {     
  }

  event: Event;
  invites: EventInvites;
  sendForm: FormViewModel;
  sendTestForm: FormViewModel;

  private invitesFormCallback: Subject<boolean> = new Subject<boolean>();
  private testFormCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    this.event = this.eventAdminService.getActiveEvent();
    this.eventAdminService.getEventInvites(this.event.id).subscribe((invites: EventInvites) => {
      this.invites = invites;
      this.buildForms();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.invitesFormCallback.complete();
    this.testFormCallback.complete();
  }

  onSendFormSubmit(): void {
    if (!confirm('Are you sure you want to send invite emails for this event?')) {
      this.invitesFormCallback.next(false);
      return;
    }

    this.sendEmails();
  }

  onSendTestFormSubmit(): void {
    if (!confirm('Are you sure you want to send a test invite email to yourself?')) {
      this.testFormCallback.next(false);
      return;
    }

    this.sendEmails(true);
  }

  private buildForms(): void {
    if (this.invites.sentDate != null) {
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
      callback: this.invitesFormCallback,
      controls: [],
      messages: {
        success: 'Invites sent'
      }
    };

    this.sendTestForm = {
      buttons: [
        {  
          text: 'Send test',
          type: 'secondary'
        }
      ],
      callback: this.testFormCallback,
      controls: [],
      messages: {
        success: 'Test invites sent'
      }
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
    this.eventAdminService.sendInvites(this.event.id, test).subscribe(() => {
      
      if (test) {
        this.testFormCallback.next(true);
      } else {
        this.invitesFormCallback.next(true);
      }      

      this.loadStatistics();
    });
  }
}
