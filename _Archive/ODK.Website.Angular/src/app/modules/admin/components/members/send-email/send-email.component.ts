import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-send-email',
  templateUrl: './send-email.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SendEmailComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private memberAdminService: MemberAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  form: FormViewModel;

  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    body: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  };
  private member: Member;

  ngOnInit(): void {
    this.member = this.memberAdminService.getActiveMember();

    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.emailAdminService.sendEmail(this.member.id, this.formControls.subject.value,
      this.formControls.body.value
    ).subscribe(() => {
      this.formCallback.next(true);
      this.buildForm();
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
      ],
      messages: {
        success: 'Email sent'
      }
    };
  }
}
