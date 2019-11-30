import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { appPaths } from 'src/app/routing/app-paths';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';

@Component({
  selector: 'app-member',
  templateUrl: './member.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterService: ChapterService,
    private memberService: MemberService
  ) {   
  }

  member: Member;
  memberId: string;

  ngOnInit(): void {
    this.memberId = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.member.params.id);
    this.changeDetector.detectChanges();

    const chapter: Chapter = this.chapterService.getActiveChapter();

    this.memberService.getMember(this.memberId, chapter.id).subscribe((member: Member) => {      
      if (!member) {
        this.router.navigateByUrl(appUrls.members(chapter));
        return;
      }

      this.member = member;
      this.changeDetector.detectChanges();
    });
  }  
}
