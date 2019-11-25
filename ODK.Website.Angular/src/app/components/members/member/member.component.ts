import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { tap, switchMap } from 'rxjs/operators';

import { appPaths } from 'src/app/routing/app-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapter/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';
import { appUrls } from 'src/app/routing/app-urls';

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

  chapter: Chapter;
  member: Member;

  ngOnInit(): void {
    const id: string = this.route.snapshot.paramMap.get(appPaths.chapter.childPaths.member.params.id);

    this.chapterService.getActiveChapter().pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap((chapter: Chapter) => this.memberService.getMember(id, chapter.id))
    ).subscribe((member: Member) => {
      if (!member) {
        this.router.navigateByUrl(appUrls.members(this.chapter));
        return;
      }

      this.member = member;
      this.changeDetector.detectChanges();
    });
  }

}
