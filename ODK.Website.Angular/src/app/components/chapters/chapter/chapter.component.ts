import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) {
  }

  chapter: Chapter;

  ngOnInit(): void {
    this.chapterService.getActiveChapter().subscribe((chapter: Chapter) => {      
      this.chapter = chapter;
      this.changeDetector.detectChanges();
    });
  }
}
