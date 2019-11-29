import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/components/structure/navbar/menu-item';

@Component({
  selector: 'app-admin-menu',
  templateUrl: './admin-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminMenuComponent implements OnInit {

  constructor(private chapterService: ChapterAdminService) { }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterService.getActiveChapter();
    this.menuItems = [
      { link: adminUrls.chapter(chapter), text: chapter.name },
      { link: adminUrls.events(chapter), text: 'Events' }
    ];
  }

}
