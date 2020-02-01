import { Component, ChangeDetectionStrategy, OnChanges, ChangeDetectorRef, Input, Output, EventEmitter } from '@angular/core';

import { Observable } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { Email } from 'src/app/core/emails/email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-email-form',
  templateUrl: './email-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailFormComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private authenticationService: AuthenticationService,
    private emailAdminService: EmailAdminService
  ) {     
  }

  @Input() chapterEmail: ChapterEmail;
  @Input() email: Email;
  @Input() formCallback: Observable<boolean | string[]>;

  @Output() formSubmit: EventEmitter<Email> = new EventEmitter<Email>();

  form: FormViewModel;
  successMessage: string;

  private formControls: {
    htmlContent: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.email) {
      this.email = this.chapterEmail;
    }

    if (!this.email) {
      return;
    }
    
    this.buildForm();
  }

  onAlertClose(): void {
    this.successMessage = null;
    this.changeDetector.detectChanges();
  }

  onFormSubmit(): void {
    this.email.htmlContent = this.formControls.htmlContent.value;
    this.email.subject = this.formControls.subject.value;
    this.formSubmit.emit(this.email);
  }

  onRestoreDefaultClick(): void {
    if (!this.chapterEmail || !this.chapterEmail.id) {
      return;
    }
    
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();

    this.form = null;
    this.changeDetector.detectChanges();

    this.emailAdminService.deleteChapterEmail(chapter.id, this.email.type).pipe(
      switchMap(() => this.emailAdminService.getChapterEmail(chapter.id, this.email.type))
    ).subscribe((email: ChapterEmail) => {
      this.chapterEmail = email;
      this.email = email;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onSendTestEmail(): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.emailAdminService.sendEmail(token.memberId, this.formControls.subject.value, this.formControls.htmlContent.value).subscribe(() => {
      this.successMessage = 'Test email sent';
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {

    this.formControls = {
      htmlContent: new HtmlEditorFormControlViewModel({
        id: `html-content`,
        label: {
          text: 'Content'
        },
        validation: {
          required: true
        },
        value: this.email.htmlContent
      }),
      subject: new TextInputFormControlViewModel({
        id: `subject`,
        label: {
          text: 'Subject'
        },
        validation: {
          required: true
        },
        value: this.email.subject
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.subject,
        this.formControls.htmlContent
      ]
    };
  }
}
