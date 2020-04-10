import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-admin-menu',
  templateUrl: './admin-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminMenuComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService) { }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();

    this.menuItems = [
      { link: appUrls.chapter(chapter), icon: 'fas fa-home', text: '' },
      { link: adminUrls.chapter(chapter), text: chapter.name },
      { link: adminUrls.events(chapter), text: 'Events' },
      { link: adminUrls.members(chapter), text: 'Members' },
      { link: adminUrls.media(chapter), text: 'Media' }
    ];
  }
}
