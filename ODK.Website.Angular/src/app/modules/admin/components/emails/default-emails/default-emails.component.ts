import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Email } from 'src/app/core/emails/email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { EmailType } from 'src/app/core/emails/email-type';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { StringUtils } from 'src/app/utils/string-utils';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-default-emails',
  templateUrl: './default-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DefaultEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {     
  }

  viewModels: { 
    email: Email;
    form: FormViewModel;
  }[];
  
  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: Map<EmailType, {  
    heading: ReadOnlyFormControlViewModel;
    htmlContent: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  }>;
  private emails: Email[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.emailAdminService.getEmails(this.chapter.id).subscribe((emails: Email[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(email: Email): void {
    email.htmlContent = this.formControls.get(email.type).htmlContent.value;
    email.subject = this.formControls.get(email.type).subject.value;

    this.formCallback.next(true);
    this.viewModels = null;
    this.changeDetector.detectChanges();

    this.emailAdminService.updateEmail(email, this.chapter.id).pipe(
      switchMap(() => this.emailAdminService.getEmails(this.chapter.id))
    ).subscribe((emails: Email[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }
  
  private buildForm(): void {
    this.viewModels = [];
    this.formControls = new Map();

    this.emails
      .sort((a, b) => EmailType[a.type].localeCompare(EmailType[b.type]))
      .forEach((email: Email) => {
        this.formControls.set(email.type, {
          heading: new ReadOnlyFormControlViewModel({
            id: `heading-${email.type}`,
            label: {
              text: StringUtils.camelPad(EmailType[email.type] || 'Unknown'),
              type: 'heading'
            }
          }),
          htmlContent: new HtmlEditorFormControlViewModel({
            id: `html-content-${email.type}`,
            label: {
              text: 'Content'
            },
            validation: {
              required: true
            },
            value: email.htmlContent
          }),
          subject: new TextInputFormControlViewModel({
            id: `subject-${email.type}`,
            label: {
              text: 'Subject'
            },
            validation: {
              required: true
            },
            value: email.subject
          })
        });
        
        const form: FormViewModel = {
          buttons: [
            { text: 'Update' }
          ],
          callback: this.formCallback,
          controls: [
            this.formControls.get(email.type).heading,
            this.formControls.get(email.type).subject,
            this.formControls.get(email.type).htmlContent
          ]
        };

        this.viewModels.push({
          email: email,
          form: form
        });
      });
  }
}
