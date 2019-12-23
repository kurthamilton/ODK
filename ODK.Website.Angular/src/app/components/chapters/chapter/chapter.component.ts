import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';
import { TitleService } from 'src/app/services/title/title.service';

@Component({
  selector: 'app-chapter',
  templateUrl: './chapter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private memberService: MemberService,
    private authenticationService: AuthenticationService,
    private titleService: TitleService
  ) {
  }

  chapter: Chapter;
  isMember: boolean;
  latestMembers: Member[];
  welcomeTextHtml: string;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.titleService.setRouteTitle(this.chapter.name);
    
    const token: AuthenticationToken = this.authenticationService.getToken();
    this.isMember = !!token && token.chapterId === this.chapter.id;

    if (this.isMember) {
      this.loadMemberPage();
    } else {
      this.loadPublicPage();
    }
  }

  private loadMemberPage(): void {
    this.memberService.getLatestMembers(this.chapter.id).subscribe((members: Member[]) => {
      this.latestMembers = members;
      this.changeDetector.detectChanges();
    });
  }

  private loadPublicPage(): void {
    this.chapterService.getChapterTexts(this.chapter.id).subscribe((details: ChapterTexts) => {
      this.welcomeTextHtml = details.welcomeText;
      this.changeDetector.detectChanges();
    });
  }
}
