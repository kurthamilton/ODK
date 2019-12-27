import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmail } from 'src/app/core/chapters/chapter-email';
import { ChapterEmailType } from 'src/app/core/chapters/chapter-email-type';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { StringUtils } from 'src/app/utils/string-utils';
import { Subject } from 'rxjs';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-emails',
  templateUrl: './chapter-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  forms: FormViewModel[];
  
  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: Map<ChapterEmailType, {  
    heading: ReadOnlyFormControlViewModel;
    htmlContent: HtmlEditorFormControlViewModel;
    subject: TextInputFormControlViewModel;
  }>;
  private emails: ChapterEmail[];
  
  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.chapterAdminService.getChapterEmails(this.chapter.id).subscribe((emails: ChapterEmail[]) => {
      this.emails = emails;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(form: FormViewModel): void {
    const emailType: ChapterEmailType = parseInt(form.id);
    
    const email: ChapterEmail = {
      htmlContent: this.formControls.get(emailType).htmlContent.value,
      id: '',
      subject: this.formControls.get(emailType).subject.value,
      type: emailType
    };

    this.chapterAdminService.updateChapterEmail(this.chapter.id, email).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.forms = [];
    this.formControls = new Map();

    this.emails
      .sort((a, b) => ChapterEmailType[a.type].localeCompare(ChapterEmailType[b.type]))
      .forEach((chapterEmail: ChapterEmail) => {
        this.formControls.set(chapterEmail.type, {
          heading: new ReadOnlyFormControlViewModel({
            id: `heading-${chapterEmail.type}`,
            label: {
              text: StringUtils.camelPad(ChapterEmailType[chapterEmail.type]),
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
        
        this.forms.push({
          buttons: [
            { text: 'Update' }
          ],
          callback: this.formCallback,
          controls: [
            this.formControls.get(chapterEmail.type).heading,
            this.formControls.get(chapterEmail.type).subject,
            this.formControls.get(chapterEmail.type).htmlContent
          ],
          id: chapterEmail.type.toString()
        });
      });
  }
}
