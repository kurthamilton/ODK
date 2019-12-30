import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { switchMap } from 'rxjs/operators';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMember } from 'src/app/core/chapters/chapter-admin-member';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';

@Component({
  selector: 'app-chapter-admin-members',
  templateUrl: './chapter-admin-members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterAdminMembersComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }
  
  adminMembers: ChapterAdminMember[];
  links: {
    addAdminMember: string;
  };

  private chapter: Chapter;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.links = {
      addAdminMember: adminUrls.chapterAdminMemberAdd(this.chapter)
    };

    this.chapterAdminService.getChapterAdminMembers(this.chapter.id).subscribe((adminMembers: ChapterAdminMember[]) => {
      this.adminMembers = adminMembers;
      this.changeDetector.detectChanges();
    });
  }

  getAdminMemberLink(adminMember: ChapterAdminMember): string {
    return adminUrls.chapterAdminMember(this.chapter, adminMember.memberId);
  }

  onRemoveAdminMember(adminMember: ChapterAdminMember): void {
    if (!confirm('Are you sure you want to remove admin access for this member?')) {
      return;
    }

    this.adminMembers = null;
    this.changeDetector.detectChanges();

    this.chapterAdminService.removeChapterAdminMember(this.chapter.id, adminMember).pipe(
      switchMap(() => this.chapterAdminService.getChapterAdminMembers(this.chapter.id))
    ).subscribe((adminMembers: ChapterAdminMember[]) => {
      this.adminMembers = adminMembers;
      this.changeDetector.detectChanges();
    })
  }
}
