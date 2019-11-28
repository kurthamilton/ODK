import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from '../../../../../../../src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/components/structure/navbar/menu-item';

@Component({
  selector: 'app-chapter-menu',
  templateUrl: './chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterMenuComponent implements OnInit {

  constructor(private chapterService: ChapterAdminService) { }

  chapterMenuItem: MenuItem;
  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.chapterMenuItem = { link: adminUrls.chapter(chapter), text: chapter.name };
    this.menuItems = [        
      { link: adminUrls.events(chapter), text: 'Events' }
    ];
  }

}
