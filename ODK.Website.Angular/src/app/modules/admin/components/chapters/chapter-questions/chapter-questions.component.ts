import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';

@Component({
  selector: 'app-chapter-questions',
  templateUrl: './chapter-questions.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterQuestionsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) { 
  }

  links: {
    create: string;
  };
  questions: ChapterQuestion[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.links = {
      create: adminUrls.chapterQuestionCreate(this.chapter)
    };

    this.chapterAdminService.getChapterQuestions(this.chapter.id).subscribe((questions: ChapterQuestion[]) => {
      this.questions = questions;
      this.changeDetector.detectChanges();
    });
  }

  getQuestionLink(question: ChapterQuestion): string {
    return adminUrls.chapterQuestion(this.chapter, question);
  }

  onDeleteQuestion(question: ChapterQuestion): void {
    if (!confirm('Are you sure you want to delete this question?')) {
      return;
    }

    this.chapterAdminService.deleteChapterQuestion(question).pipe(
      switchMap(() => this.chapterAdminService.getChapterQuestions(this.chapter.id))
    ).subscribe((questions: ChapterQuestion[]) => {
      this.questions = questions;
      this.changeDetector.detectChanges();
    })
  }

  onMoveQuestionDown(question: ChapterQuestion): void {
    this.chapterAdminService.moveChapterQuestionDown(question.id).subscribe((questions: ChapterQuestion[]) => {
      this.questions = questions;
      this.changeDetector.detectChanges();
    });
  }

  onMoveQuestionUp(question: ChapterQuestion): void {
    this.chapterAdminService.moveChapterQuestionUp(question.id).subscribe((questions: ChapterQuestion[]) => {
      this.questions = questions;
      this.changeDetector.detectChanges();
    });
  }
}
