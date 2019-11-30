import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-members',
  templateUrl: './members.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MembersComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {     
  }

  chapter: Chapter;
  members: Member[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.memberService.getMembers(this.chapter.id).subscribe((members: Member[]) => {
      this.members = members.sort((a, b) => a.fullName.localeCompare(b.fullName));
      this.changeDetector.detectChanges();
    });
  }
}
