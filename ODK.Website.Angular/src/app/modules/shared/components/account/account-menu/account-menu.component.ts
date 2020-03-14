import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';

import { takeUntil, filter } from 'rxjs/operators';

import { accountUrls } from 'src/app/modules/account/routing/account-urls';
import { adminUrls } from 'src/app/modules/admin/routing/admin-urls';
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
    private router: Router,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) {
  }

  activeChapter: Chapter;
  authenticated: boolean;
  chapter: Chapter;

  links: {
    admin: {
      eventCreate: string;
      events: string;
      superAdmin: string;
    };
    chapter: string,
    logout: string;
    profile: string
  };

  joinLink: string;
  loginLink: string;
  loginQueryParams: {
    returnUrl: string;
  };

  private lastUrl: string;

  ngOnInit(): void {
    this.lastUrl = this.router.url;

    this.chapterService.activeChapterChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((chapter: Chapter) => {
      this.setActiveChapter(chapter);
      this.changeDetector.detectChanges();
    });

    this.authenticationService.authenticationTokenChange().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((token: AuthenticationToken) => {
      this.onAuthenticationChange(token);
    });

    this.authenticationService.authenticationExpired().pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe(() => {
      this.router.navigate([this.loginLink], { queryParams: this.loginQueryParams });
    });

    this.router.events.pipe(
      takeUntil(componentDestroyed(this)),
      filter((event) => event instanceof NavigationEnd)
    ).subscribe((event: NavigationEnd) => {
      this.lastUrl = event.url;
      this.setLoginParams();

      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {}

  private onAuthenticationChange(token: AuthenticationToken): void {
    if (!token) {
      this.authenticated = false;
      this.links = null;
      this.changeDetector.detectChanges();
      return;
    }

    this.authenticated = true;

    this.chapterService.getChapterById(token.chapterId).subscribe((chapter: Chapter) => {
      this.chapter = chapter;
      this.links = {
        admin: null,
        chapter: appUrls.chapter(chapter),
        logout: accountUrls.logout(chapter),
        profile: accountUrls.profile(chapter)
      };

      if (token.adminChapterIds && token.adminChapterIds.includes(chapter.id)) {
        this.links.admin = {
          eventCreate: adminUrls.eventCreate(chapter),
          events: adminUrls.events(chapter),
          superAdmin: token.superAdmin ? adminUrls.superAdminPaymentSettings(chapter) : null
        };
      }

      this.changeDetector.detectChanges();
    });
  }

  private setActiveChapter(chapter: Chapter): void {
    this.activeChapter = chapter;
    this.loginLink = appUrls.login(chapter);
    this.joinLink = accountUrls.join(chapter);
    this.setLoginParams();
  }

  private setLoginParams(): void {
    const lastUrl: string = this.lastUrl.split('?')[0];
    this.loginQueryParams = {
      returnUrl: lastUrl === appUrls.login(this.activeChapter) ? appUrls.home(this.activeChapter) : this.lastUrl
    };
  }
}
