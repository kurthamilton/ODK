import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterDetails } from 'src/app/core/chapters/chapter-details';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-chapter-header',
  templateUrl: './chapter-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterHeaderComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) { 
  }
  
  chapter: Chapter;
  chapterDetails: ChapterDetails;
  links: {
    chapter: string
  };

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.chapterService.getChapterDetails(this.chapter.id).subscribe((chapterDetails: ChapterDetails) => {
      
      this.chapterDetails = chapterDetails;
      this.links = {
        chapter: appUrls.chapter(this.chapter)
      };

      this.changeDetector.detectChanges();
    });
  }

}
