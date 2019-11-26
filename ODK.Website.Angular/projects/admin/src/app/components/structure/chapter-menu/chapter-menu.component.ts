import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from '../../../services/chapters/chapter.service';
import { MenuItem } from 'src/app/components/structure/navbar/menu-item';

@Component({
  selector: 'app-chapter-menu',
  templateUrl: './chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterMenuComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService
  ) { 
  }

  chapterMenuItem: MenuItem;
  menuItems: MenuItem[];

  ngOnInit(): void {
    this.chapterService.getActiveChapter().subscribe((chapter: Chapter) => {
      this.chapterMenuItem = { link: adminUrls.chapter(chapter), text: chapter.name };
      this.menuItems = [        
        { link: adminUrls.events(chapter), text: 'Events' }
      ];
      this.changeDetector.detectChanges();
    });
  }

}
