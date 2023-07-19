import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-member-layout',
  templateUrl: './member-layout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberLayoutComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  member: Member;

  private chapter: Chapter;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(adminPaths.members.member.params.id);
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.breadcrumbs = [
      { link: adminUrls.members(this.chapter), text: 'Members' }
    ];

    this.memberAdminService.getAdminMember(id).subscribe((member: Member) => {
      this.member = member;
      this.memberAdminService.setActiveMember(member);
      this.changeDetector.detectChanges();
    });
  }

}
