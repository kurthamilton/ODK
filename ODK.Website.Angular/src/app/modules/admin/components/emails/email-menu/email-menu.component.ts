import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-email-menu',
  templateUrl: './email-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EmailMenuComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService,
    private authenticationService: AuthenticationService
  ) { 
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const token: AuthenticationToken = this.authenticationService.getToken();

    this.menuItems = [
      { link: adminUrls.emails(chapter), text: 'Chapter', matchExactRoute: true }
    ];

    if (token.superAdmin) {
      this.menuItems.push(...[
        { link: adminUrls.emailProviders(chapter), text: 'Providers' },
        { link: adminUrls.emailsDefault(chapter), text: 'Default' }
      ]);
    }
  }

}
