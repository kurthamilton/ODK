import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmail } from 'src/app/core/emails/chapter-email';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { EmailType } from 'src/app/core/emails/email-type';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { StringUtils } from 'src/app/utils/string-utils';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-emails',
  templateUrl: './chapter-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService,
    private authenticationService: AuthenticationService
  ) {     
  }

  viewModels: { 
    email: ChapterEmail;
    form: FormViewModel;
  }[];
  
  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: Map<EmailType, {  
    heading: ReadOnlyFormControlViewModel;
    htmlContent: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  }>;
  private emails: ChapterEmail[];
  
  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.emailAdminService.getChapterEmails(this.chapter.id).subscribe((emails: ChapterEmail[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(email: ChapterEmail): void {
    email.htmlContent = this.formControls.get(email.type).htmlContent.value;
    email.subject = this.formControls.get(email.type).subject.value;

    this.formCallback.next(true);
    this.viewModels = null;
    this.changeDetector.detectChanges();

    this.emailAdminService.updateChapterEmail(this.chapter.id, email).pipe(
      switchMap(() => this.emailAdminService.getChapterEmails(this.chapter.id))
    ).subscribe((emails: ChapterEmail[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onRestoreDefaultClick(email: ChapterEmail): void {
    this.viewModels = null;

    this.emailAdminService.deleteChapterEmail(this.chapter.id, email.type).pipe(
      switchMap(() => this.emailAdminService.getChapterEmails(this.chapter.id))
    ).subscribe((emails: ChapterEmail[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }
  
  onSendTestEmail(email: ChapterEmail): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.emailAdminService.sendEmail(token.memberId, email.subject, email.htmlContent).subscribe(() => {

    });
  }

  private buildForm(): void {
    this.viewModels = [];
    this.formControls = new Map();

    this.emails
      .sort((a, b) => EmailType[a.type].localeCompare(EmailType[b.type]))
      .forEach((chapterEmail: ChapterEmail) => {
        this.formControls.set(chapterEmail.type, {
          heading: new ReadOnlyFormControlViewModel({
            id: `heading-${chapterEmail.type}`,
            label: {
              text: StringUtils.camelPad(EmailType[chapterEmail.type] || 'Unknown'),
              type: 'heading'
            }
          }),
          htmlContent: new HtmlEditorFormControlViewModel({
            id: `html-content-${chapterEmail.type}`,
            label: {
              text: 'Content'
            },
            validation: {
              required: true
            },
            value: chapterEmail.htmlContent
          }),
          subject: new TextInputFormControlViewModel({
            id: `subject-${chapterEmail.type}`,
            label: {
              text: 'Subject'
            },
            validation: {
              required: true
            },
            value: chapterEmail.subject
          })
        });
        
        const form: FormViewModel = {
          buttons: [
            { text: 'Update' }
          ],
          callback: this.formCallback,
          controls: [
            this.formControls.get(chapterEmail.type).heading,
            this.formControls.get(chapterEmail.type).subject,
            this.formControls.get(chapterEmail.type).htmlContent
          ]
        };

        this.viewModels.push({
          email: chapterEmail,
          form: form
        });
      });
  }
}