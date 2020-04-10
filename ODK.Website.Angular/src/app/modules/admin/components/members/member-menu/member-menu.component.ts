import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-member-menu',
  templateUrl: './member-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberMenuComponent implements OnInit {

  constructor(
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService
  ) {
  }

  menuItems: MenuItem[];

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const member: Member = this.memberAdminService.getActiveMember();

    this.menuItems = [
      { link: adminUrls.member(chapter, member), text: 'Subscription', matchExactRoute: true },
      { link: adminUrls.memberImage(chapter, member), text: 'Image' },
      { link: adminUrls.memberEvents(chapter, member), text: 'Events' },
      { link: adminUrls.memberSendEmail(chapter, member), text: 'Send email' }
    ];
  }

}
