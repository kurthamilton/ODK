import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private memberAdminService: MemberAdminService,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  member: Member;
  updateImage: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    const id: string = this.route.snapshot.paramMap.get(adminPaths.members.member.params.id);
    this.memberAdminService.getMember(id, chapter.id).subscribe((member: Member) => {
      this.member = member;
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.updateImage.complete();
  }

  onRotate(): void {
    this.memberAdminService.rotateMemberImage(this.member.id)
      .subscribe(() => this.updateImage.next(true));
  }
}
