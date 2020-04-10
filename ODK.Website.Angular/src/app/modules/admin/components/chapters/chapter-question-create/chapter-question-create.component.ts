import { Component, OnInit, ChangeDetectionStrategy, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-question-create',
  templateUrl: './chapter-question-create.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterQuestionCreateComponent implements OnInit, OnDestroy {

  constructor(
    private router: Router,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  formCallback: Subject<string[]> = new Subject<string[]>();
  question: ChapterQuestion;

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.question = this.createEmptyQuestion();
    this.breadcrumbs = [
      { link: adminUrls.chapterQuestions(this.chapter), text: 'Questions' }
    ];
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(question: ChapterQuestion): void {
    this.chapterAdminService.createChapterQuestion(this.chapter.id, question).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      if (result.success) {
        this.router.navigateByUrl(adminUrls.chapterQuestions(this.chapter));
      }
    });
  }

  private createEmptyQuestion(): ChapterQuestion {
    return {
      answer: '',
      id: '',
      name: ''
    };
  }
}
