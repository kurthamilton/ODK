import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-chapter-header',
  templateUrl: './chapter-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterHeaderComponent implements OnInit {

  constructor(private chapterService: ChapterService) { 
  }
  
  chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }
}
