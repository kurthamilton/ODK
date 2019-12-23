import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { HtmlEditorFormControlViewModel } from '../../forms/inputs/html-editor-form-control/html-editor-form-control.view-model';

@Component({
  selector: 'app-chapter-settings',
  templateUrl: './chapter-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterSettingsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  form: FormViewModel;

  private chapter: Chapter;
  private chapterTexts: ChapterTexts;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    registerText: HtmlEditorFormControlViewModel;
    welcomeText: HtmlEditorFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterTexts(this.chapter.id).subscribe((texts: ChapterTexts) => {
      this.chapterTexts = texts;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.chapterTexts.registerText = this.formControls.registerText.value;
    this.chapterTexts.welcomeText = this.formControls.welcomeText.value;

    this.chapterAdminService.updateChapterTexts(this.chapter.id, this.chapterTexts).subscribe((texts: ChapterTexts) => {
      this.chapterTexts = texts;
      this.buildForm();
      this.changeDetector.detectChanges();

      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      registerText: new HtmlEditorFormControlViewModel({
        id: 'register-text',
        label: {
          helpText: 'The message to display on the register page',
          text: 'Register message'
        },
        validation: {
          required: true
        },
        value: this.chapterTexts.registerText
      }),
      welcomeText: new HtmlEditorFormControlViewModel({
        id: 'welcome-text',
        label: {
          helpText: 'The message to display to non-members on the chapter home page',
          text: 'Welcome message'
        },
        validation: {
          required: true
        },
        value: this.chapterTexts.welcomeText
      })
    };

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback,
      controls: [
        this.formControls.registerText,
        this.formControls.welcomeText
      ]
    };
  }

}
