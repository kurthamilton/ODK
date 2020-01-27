import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-admin-menu',
  templateUrl: './admin-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AdminMenuComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService,
    private authenticationService: AuthenticationService
  ) {     
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const token: AuthenticationToken = this.authenticationService.getToken();

    this.menuItems = [
      { link: adminUrls.chapter(chapter), text: chapter.name },
      { link: adminUrls.events(chapter), text: 'Events' },
      { link: adminUrls.venues(chapter), text: 'Venues' },
      { link: adminUrls.members(chapter), text: 'Members' },
      { link: adminUrls.emails(chapter), text: 'Emails' },
      { link: adminUrls.media(chapter), text: 'Media' }
    ];

    if (token.superAdmin === true) {
      this.menuItems.push({
        link: adminUrls.adminLog(chapter),
        text: 'Admin'
      });
    }
  }
}
