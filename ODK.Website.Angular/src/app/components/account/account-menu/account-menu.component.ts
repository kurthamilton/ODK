import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-account-menu',
  templateUrl: './account-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountMenuComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) { 
  }

  authenticated: boolean;
  chapter: Chapter;

  links: {
    admin: string;
    chapter: string,
    logout: string;
    profile: string
  };

  loginLink: string;

  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnInit(): void {
    this.authenticationService.isAuthenticated().pipe(
      takeUntil(this.destroyed)
    ).subscribe((token: AuthenticationToken) => this.onAuthenticationChange(token));
  }

  ngOnDestroy(): void {
    this.destroyed.next();
  }

  private onAuthenticationChange(token: AuthenticationToken): void {
    if (!token) {
      this.authenticated = false;
      this.links = null;
      this.loginLink = appUrls.login;
      this.changeDetector.detectChanges();
      return;
    }

    this.authenticated = true;

    this.chapterService.getChapterById(token.chapterId).subscribe((chapter: Chapter) => {
      this.chapter = chapter;
      this.links = {
        admin: token.adminChapterIds.includes(chapter.id) ? appUrls.adminChapter(chapter) : null,
        chapter: appUrls.chapter(chapter),        
        logout: `/${appUrls.logout}`,
        profile: appUrls.profile(chapter)
      };

      this.changeDetector.detectChanges();
    });
  }
}
