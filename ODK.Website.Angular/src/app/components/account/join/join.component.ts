import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { Chapter } from 'src/app/core/chapters/chapter';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-join',
  templateUrl: './join.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class JoinComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {     
  }

  form: FormViewModel;

  private chapter: Chapter;
  private formControls: {
    emailAddress: TextInputFormControlViewModel;
    firstName: TextInputFormControlViewModel;
    lastName: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

  }

  private buildForm(): void {

  }
}
