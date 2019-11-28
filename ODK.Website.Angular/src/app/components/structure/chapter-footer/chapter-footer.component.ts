import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-chapter-footer',
  templateUrl: './chapter-footer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterFooterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) { 
  }

  links: ChapterLinks;

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.chapterService.getChapterLinks(chapter.id).subscribe((links: ChapterLinks) => {   
      this.links = links;      
      this.changeDetector.detectChanges();
    });
  }

}
