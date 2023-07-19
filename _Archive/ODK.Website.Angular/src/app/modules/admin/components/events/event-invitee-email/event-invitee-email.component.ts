import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { DropDownMultiFormControlViewModel } from '../../forms/inputs/drop-down-multi-form-control/drop-down-multi-form-control.view-model';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-event-invitee-email',
  templateUrl: './event-invitee-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventInviteeEmailComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService
  ) {
  }

  form: FormViewModel;

  private event: Event;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    body: HtmlEditorFormControlViewModel;
    status: DropDownMultiFormControlViewModel;
    subject: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.event = this.eventAdminService.getActiveEvent();
    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    const statuses: EventResponseType[] = this.formControls.status.value.map(x => parseInt(x, 10));
    this.eventAdminService
      .sendInviteeEmail(this.event.id, statuses, this.formControls.subject.value, this.formControls.body.value)
      .subscribe(() => {
        this.formCallback.next(true);
        this.changeDetector.detectChanges();
      });
  }

  private buildForm(): void {
    this.formControls = {
      body: new HtmlEditorFormControlViewModel({
        id: 'body',
        label: {
          text: 'Body'
        },
        validation: {
          required: true
        }
      }),
      status: new DropDownMultiFormControlViewModel({
        id: 'status',
        label: {
          text: 'Response status'
        },
        options: [
          { text: 'Going', value: EventResponseType.Yes.toString() },
          { text: 'Maybe', value: EventResponseType.Maybe.toString() },
          { text: 'Declined', value: EventResponseType.No.toString() },
          { text: 'No response', value: EventResponseType.None.toString() },
          { text: 'Not invited', value: EventResponseType.NotInvited.toString() }
        ],
        validation: {
          required: true
        }
      }),
      subject: new TextInputFormControlViewModel({
        id: 'subject',
        label: {
          text: 'Subject'
        },
        validation: {
          required: true
        }
      })
    };

    this.form = {
      buttons: [
        { text: 'Send' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.status,
        this.formControls.subject,
        this.formControls.body
      ],
      messages: {
        success: 'Email sent'
      }
    };
  }
}
