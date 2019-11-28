import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { MenuItem } from '../navbar/menu-item';

@Component({
  selector: 'app-home-menu',
  templateUrl: './home-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomeMenuComponent implements OnInit {
  constructor(private changeDetector: ChangeDetectorRef, 
    private chapterService: ChapterService) { 
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    this.chapterService.getChapters().subscribe((chapters: Chapter[]) => {
      this.menuItems = chapters.map((chapter: Chapter): MenuItem => ({
        externalLink: chapter.redirectUrl ? chapter.redirectUrl : null,
        link: !chapter.redirectUrl ? appUrls.chapter(chapter) : null,
        text: chapter.name
      }));
      this.changeDetector.detectChanges();
    });
  }
}
