import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

import { takeUntil, filter } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';

@Component({
  selector: 'app-account-menu',
  templateUrl: './account-menu.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccountMenuComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private router: Router
  ) {
  }

  activeChapter: Chapter;
  authenticated: boolean;
  chapter: Chapter;

  links: {
    admin: string;
    chapter: string,
    logout: string;
    profile: string
  };

  joinLink: string;
  loginLink: string;
  loginQueryParams: {};

  ngOnInit(): void {
    this.chapterService.activeChapterChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((chapter: Chapter) => {
      this.activeChapter = chapter;
      this.changeDetector.detectChanges();
    });

    this.authenticationService.authenticationTokenChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((token: AuthenticationToken) => {
      this.onAuthenticationChange(token);
    });

    this.router.events.pipe(
      takeUntil(componentDestroyed(this)),
      filter((event) => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.setLoginLink(event.url);
      this.changeDetector.detectChanges();
    });
  }


  ngOnDestroy(): void {}

  private onAuthenticationChange(token: AuthenticationToken): void {
    this.setLoginLink(this.router.url);

    if (!token) {
      this.authenticated = false;
      this.links = null;
      this.joinLink = appUrls.join;      
      this.changeDetector.detectChanges();
      return;
    }

    this.authenticated = true;

    this.chapterService.getChapterById(token.chapterId).subscribe((chapter: Chapter) => {
      this.chapter = chapter;
      this.links = {
        admin: token.adminChapterIds && token.adminChapterIds.includes(chapter.id) ? appUrls.adminChapter(chapter) : null,
        chapter: appUrls.chapter(chapter),
        logout: `/${appUrls.logout}`,
        profile: appUrls.profile(chapter)
      };

      this.changeDetector.detectChanges();
    });
  }

  private setLoginLink(url: string): void {
    if (this.authenticated) {
      this.loginLink = null;
    }

    this.loginLink = appUrls.login;
    this.loginQueryParams = {
      returnUrl: url
    };
  }
}
