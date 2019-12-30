import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterLinks } from 'src/app/core/chapters/chapter-links';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { ChapterTexts } from 'src/app/core/chapters/chapter-texts';
import { Member } from 'src/app/core/members/member';
import { MemberService } from 'src/app/services/members/member.service';
import { SocialMediaImage } from 'src/app/core/social-media/social-media-image';
import { SocialMediaService } from 'src/app/services/social-media/social-media.service';
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
    private titleService: TitleService,
    private socialMediaService: SocialMediaService
  ) {
  }

  chapter: Chapter;
  instagramImages: SocialMediaImage[];
  isMember: boolean;
  latestMembers: Member[];
  links: ChapterLinks;
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

    forkJoin([
      this.socialMediaService.getChapterInstagramImages(this.chapter.id, 8).pipe(
        tap((images) => this.instagramImages = images)
      ),
      this.chapterService.getChapterLinks(this.chapter.id).pipe(
        tap((links: ChapterLinks) => this.links = links)
      )
    ]).subscribe(() => {
      this.changeDetector.detectChanges();
    });
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
