import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-events-admin-layout',
  templateUrl: './events-admin-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EventsAdminLayoutComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService) { }

  menuItems: MenuItem[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.menuItems = [
      { link: adminUrls.events(this.chapter), text: 'Events', matchExactRoute: true },
      { link: adminUrls.venues(this.chapter), text: 'Venues' }
    ];
  }

}
