import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { ArrayUtils } from 'src/app/utils/array-utils';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MemberEmail } from 'src/app/core/members/member-email';
import { MemberEmailViewModel } from './member-email.view-model';

@Component({
  selector: 'app-member-emails',
  templateUrl: './member-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberEmailsComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private memberAdminService: MemberAdminService
  ) {
  }

  memberEmails: MemberEmailViewModel[];

  private chapter: Chapter;
  private emails: MemberEmail[];
  private members: Member[];

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.memberAdminService.getAdminMembers(this.chapter.id).pipe(
        tap(members => this.members = members)
      ),
      this.memberAdminService.getMemberEmails(this.chapter.id).pipe(
        tap(emails => this.emails = emails)
      )
    ]).subscribe(() => {
      const emailMap: Map<string, MemberEmail>  = ArrayUtils.toMap(this.emails, x => x.memberId);
      this.memberEmails = this.members
        .sort((a, b) => a.fullName.localeCompare(b.fullName))
        .map((x: Member): MemberEmailViewModel => ({
          member: x,
          memberEmail: emailMap.get(x.id)
        }));

      this.changeDetector.detectChanges();
    });
  }
}
