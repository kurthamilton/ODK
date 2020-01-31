import { Component, ChangeDetectionStrategy, OnInit } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-chapter-admin-layout',
  templateUrl: './chapter-admin-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterAdminLayoutComponent implements OnInit {

  constructor(private chapterAdminService: ChapterAdminService) { }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    
    this.menuItems = [
      { link: adminUrls.chapter(chapter), text: 'Text', matchExactRoute: true },
      { link: adminUrls.chapterQuestions(chapter), text: 'About' },
      { link: adminUrls.chapterProperties(chapter), text: 'Properties' },
      { link: adminUrls.chapterEmails(chapter), text: 'Emails' }
    ];
  }
}
