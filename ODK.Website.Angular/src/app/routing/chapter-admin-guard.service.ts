import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router, CanLoad } from '@angular/router';

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { RouteGuardService } from 'src/app/routing/route-guard.service';
import { ChapterAdminService } from '../services/chapters/chapter-admin.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminGuardService extends RouteGuardService implements CanLoad {

  constructor(router: Router,
    private chapterAdminService: ChapterAdminService
  ) {
    super(router);
  }

  canLoad(): Observable<boolean> {
    return this.chapterAdminService.getChapters().pipe(
      map((chapters: Chapter[]) => chapters.length > 0)
    )
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const name: string = route.paramMap.get('chapter');
    return this.chapterAdminService.getChapter(name).pipe(
      tap((chapter: Chapter) => this.chapterAdminService.setActiveChapter(chapter)),
      map((chapter: Chapter) => !!chapter)
    );
  }
}
