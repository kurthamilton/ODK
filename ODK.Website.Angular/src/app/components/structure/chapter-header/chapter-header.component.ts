import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-chapter-header',
  templateUrl: './chapter-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterHeaderComponent implements OnInit {

  constructor(private chapterService: ChapterService) { }

  chapter: Chapter;
  links: {
    chapter: string
  };

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.links = {
      chapter: appUrls.chapter(this.chapter)
    };
  }

}
