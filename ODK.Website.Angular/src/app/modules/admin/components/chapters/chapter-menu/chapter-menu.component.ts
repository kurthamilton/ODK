import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-chapter-menu',
  templateUrl: './chapter-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterMenuComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService,
    private authenticationService: AuthenticationService
  ) {     
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const token: AuthenticationToken = this.authenticationService.getToken();

    this.menuItems = [
      { link: adminUrls.chapter(chapter), text: 'Settings', matchExactRoute: true },
      { link: adminUrls.chapterSubscriptions(chapter), text: 'Subscriptions' },
      { link: adminUrls.chapterQuestions(chapter), text: 'About' },
      { link: adminUrls.chapterProperties(chapter), text: 'Properties' },
      { link: adminUrls.chapterAdminMembers(chapter), text: 'Admin members' }
    ];

    if (token.superAdmin) {
      this.menuItems.push({ link: adminUrls.chapterPayments(chapter), text: 'Payments' });      
    }
  }
}
