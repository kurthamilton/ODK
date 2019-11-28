import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {
  }

  chapter: Chapter;
  latestMembers: Member[];

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.memberService.getLatestMembers(this.chapter.id).subscribe((members: Member[]) => {
      this.latestMembers = members;
      this.changeDetector.detectChanges();
    });
  }
}
