import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { MenuItem } from 'src/app/components/structure/navbar/menu-item';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';

@Component({
  selector: 'app-admin-chapter-menu',
  templateUrl: './admin-chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminChapterMenuComponent implements OnInit {

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
