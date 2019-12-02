import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { adminPaths } from 'src/app/admin/routing/admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private memberAdminService: MemberAdminService,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  member: Member;

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const id: string = this.route.snapshot.paramMap.get(adminPaths.members.member.params.id);
    this.memberAdminService.getMember(id, chapter.id).subscribe((member: Member) => {
      this.member = member;
      this.changeDetector.detectChanges();
    });
  }

  onRotate(): void {
    this.memberAdminService.rotateMemberImage(this.member.id, 90).subscribe(() => {
      // TODO: move into new admin member-image component
      const member: Member = this.member;
      this.member = null;
      this.changeDetector.detectChanges();

      this.member = member;
      this.changeDetector.detectChanges();
    });
  }
}
