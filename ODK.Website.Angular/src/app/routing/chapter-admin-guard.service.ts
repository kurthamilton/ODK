import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, CanLoad } from '@angular/router';

import { Observable } from 'rxjs';
import { map, switchMap } from 'rxjs/operators';

import { appPaths } from './app-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from '../services/chapters/chapter-admin.service';
import { ChapterService } from '../services/chapters/chapter.service';
import { RouteGuardService } from 'src/app/routing/route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminGuardService extends RouteGuardService implements CanLoad {

  constructor(router: Router,
    private chapterService: ChapterService,
    private chapterAdminService: ChapterAdminService
  ) {
    super(router);
  }

  canLoad(): Observable<boolean> {
    return this.chapterAdminService.getAdminChapters().pipe(
      map((chapters: Chapter[]) => chapters.length > 0)
    )
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const name: string = route.paramMap.get(appPaths.admin.params.chapter);
    return this.chapterService.getChapter(name).pipe(
      switchMap((chapter: Chapter) => {
        this.chapterAdminService.setActiveChapter(chapter);
        return this.chapterAdminService.hasAccess(chapter);
      })
    );
  }
}
