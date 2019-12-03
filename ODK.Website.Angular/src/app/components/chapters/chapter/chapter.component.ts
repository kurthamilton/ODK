import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterDetails } from 'src/app/core/chapters/chapter-details';
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
    private memberService: MemberService,
    private authenticationService: AuthenticationService,
    private sanitizer: DomSanitizer
  ) {
  }

  chapter: Chapter;
  latestMembers: Member[];
  welcomeText: SafeHtml;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

    const token: AuthenticationToken = this.authenticationService.getToken();

    if (token) {
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
    this.chapterService.getChapterDetails(this.chapter.id).subscribe((details: ChapterDetails) => {
      this.welcomeText = this.sanitizer.bypassSecurityTrustHtml(details.welcomeText);
      this.changeDetector.detectChanges();
    });
  }
}
