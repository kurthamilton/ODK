import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterDetails } from 'src/app/core/chapters/chapter-details';
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
  private chapterDetails: ChapterDetails;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    welcomeText: HtmlEditorFormControlViewModel
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterDetails(this.chapter.id).subscribe((details: ChapterDetails) => {
      this.chapterDetails = details;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.chapterDetails.welcomeText = this.formControls.welcomeText.value;

    this.chapterAdminService.updateChapterDetails(this.chapter.id, this.chapterDetails).subscribe((details: ChapterDetails) => {
      this.chapterDetails = details;
      this.buildForm();
      this.changeDetector.detectChanges();

      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      welcomeText: new HtmlEditorFormControlViewModel({
        id: 'welcome-text',
        label: {
          helpText: 'The message to display to non-members on the chapter home page',
          text: 'Welcome message'
        },
        validation: {
          required: true
        },
        value: this.chapterDetails.welcomeText
      })
    };

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback,
      controls: [
        this.formControls.welcomeText
      ]
    };
  }

}
