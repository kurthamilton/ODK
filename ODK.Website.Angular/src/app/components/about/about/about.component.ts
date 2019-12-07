import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterQuestion } from 'src/app/core/chapters/chapter-question';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AboutComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {     
  }

  links: {
    contact: string;
  };
  questions: ChapterQuestion[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    
    this.links = {
      contact: appUrls.contact(chapter)
    }
    this.chapterService.getChapterQuestions(chapter.id).subscribe((questions: ChapterQuestion[]) => {
      this.questions = questions;
      this.changeDetector.detectChanges();
    });
  }
}
