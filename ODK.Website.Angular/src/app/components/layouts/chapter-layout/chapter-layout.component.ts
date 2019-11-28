import { Component, OnInit, ChangeDetectionStrategy, Input } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-chapter-layout',
  templateUrl: './chapter-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterLayoutComponent implements OnInit {
  constructor(private chapterService: ChapterService) {
  }

  @Input() title: string;
  
  chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
  }
}
