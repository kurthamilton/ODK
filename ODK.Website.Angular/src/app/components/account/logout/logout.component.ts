import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';

import { switchMap, tap } from 'rxjs/operators';

import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LogoutComponent implements OnInit {

  constructor(private router: Router,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService
  ) {
  }

  private chapter: Chapter;

  ngOnInit(): void {
    const token: AuthenticationToken = this.authenticationService.getToken();
    if (!token) {
      this.router.navigateByUrl(appUrls.home(null));
      return;
    }

    this.chapterService.getChapterById(token.chapterId).pipe(
      tap((chapter: Chapter) => this.chapter = chapter),
      switchMap(() => this.authenticationService.logout())
    ).subscribe(() => {
      this.router.navigateByUrl(appUrls.home(this.chapter));
    });
  }
}
