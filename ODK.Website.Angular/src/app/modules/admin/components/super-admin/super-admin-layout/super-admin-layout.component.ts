import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';

@Component({
  selector: 'app-super-admin-layout',
  templateUrl: './super-admin-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SuperAdminLayoutComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService) { }

  menuItems: MenuItem[];

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.menuItems = [
      { link: adminUrls.emailProviders(this.chapter), text: 'Email providers' },
      { link: adminUrls.superAdminEmails(this.chapter), text: 'Emails' },
      { link: adminUrls.superAdminPaymentSettings(this.chapter), text: 'Payment settings' },
      { link: adminUrls.superAdminErrorLog(this.chapter), text: 'Error log' }
    ];
  }

}
