import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { appPaths } from './app-paths';
import { AuthenticationService } from '../services/authentication/authentication.service';
import { AuthenticationToken } from '../core/authentication/authentication-token';
import { Chapter } from '../core/chapters/chapter';
import { ChapterService } from '../services/chapters/chapter.service';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterGuardService extends RouteGuardService {

  constructor(
    router: Router,
    private chapterService: ChapterService,
    private authenticationService: AuthenticationService
  ) {
    super(router);
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const name: string = route.paramMap.get(appPaths.chapter.params.chapter);
    return this.chapterService.getChapter(name).pipe(
      tap((chapter: Chapter) => this.chapterService.setActiveChapter(chapter)),
      map((chapter: Chapter) => this.hasChapterAccess(chapter))
    );
  }

  private hasChapterAccess(chapter: Chapter): boolean {
    if (!chapter) {
      return false;
    }

    if (!chapter.redirectUrl) {
      return true;
    }

    const token: AuthenticationToken = this.authenticationService.getToken();
    return token && token.adminChapterIds && token.adminChapterIds.includes(chapter.id);
  }
}
