import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { Event } from 'src/app/core/events/event';
import { EventAdminService } from 'src/app/services/events/event-admin.service';
import { EventResponseType } from 'src/app/core/events/event-response-type';
import { FormControlLabelViewModel } from 'src/app/modules/forms/components/form-control-label/form-control-label.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-event-invitee-email',
  templateUrl: './event-invitee-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventInviteeEmailComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private eventAdminService: EventAdminService
  ) {     
  }

  form: FormViewModel;
  sent: boolean;
  statusLabel: FormControlLabelViewModel = {
    text: 'Response status'
  };
  statusOptions: DropDownFormControlOption[] = [];

  private event: Event;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    body: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  }
  private selectedStatusOptions: DropDownFormControlOption[];

  ngOnInit(): void {
    this.event = this.eventAdminService.getActiveEvent();
    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onAlertClose(): void {
    this.sent = false;
    this.changeDetector.detectChanges();
  }

  onFormSubmit(): void {
    if (this.selectedStatusOptions.length === 0) {
      return;
    }

    const statuses: EventResponseType[] = this.selectedStatusOptions.map(x => parseInt(x.value));
    this.eventAdminService
      .sendInviteeEmail(this.event.id, statuses, this.formControls.subject.value, this.formControls.body.value)
      .subscribe(() => {
        this.formCallback.next(true);
        this.sent = true;
        this.changeDetector.detectChanges();
      });
  }

  onStatusChange(selectedOptions: DropDownFormControlOption[]): void {
    this.selectedStatusOptions = selectedOptions;    
  }

  private buildForm(): void {
    this.statusOptions = [
      { text: 'Going', value: EventResponseType.Yes.toString() },
      { text: 'Maybe', value: EventResponseType.Maybe.toString() },
      { text: 'Declined', value: EventResponseType.No.toString() },
      { text: 'No response', value: EventResponseType.None.toString() },
      { text: 'Not invited', value: EventResponseType.NotInvited.toString() }
    ];

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
        this.formControls.subject,
        this.formControls.body
      ]
    };
  }
}
