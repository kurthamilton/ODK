import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-member-admin-layout',
  templateUrl: './member-admin-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberAdminLayoutComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService) { }

  menuItems: MenuItem[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.menuItems = [
      { link: adminUrls.members(this.chapter), text: 'Members' },
      { link: adminUrls.subscriptions(this.chapter), text: 'Subscriptions' },
      { link: adminUrls.adminMembers(this.chapter), text: 'Admin members' }
    ]
  }
}
