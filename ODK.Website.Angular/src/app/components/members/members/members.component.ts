import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { tap, switchMap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
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
    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.memberService.getMembers(chapter.id))
    ).subscribe((members: Member[]) => {
      this.members = members;
      this.changeDetector.detectChanges();
    });
  }
}
