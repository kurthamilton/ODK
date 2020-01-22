import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-chapter-question',
  templateUrl: './chapter-question.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterQuestionComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
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

    const questionId: string = this.route.snapshot.paramMap.get(adminPaths.chapter.questions.question.params.id);

    this.chapterAdminService.getChapterQuestion(questionId).subscribe((question: ChapterQuestion) => {
      this.question = question;

      if (!this.question) {
        this.router.navigateByUrl(adminUrls.chapterQuestions(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.chapterQuestions(this.chapter), text: 'Questions' }
      ];
      
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(question: ChapterQuestion): void {
    this.chapterAdminService.updateChapterQuestion(question).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
    });
  }  
}
