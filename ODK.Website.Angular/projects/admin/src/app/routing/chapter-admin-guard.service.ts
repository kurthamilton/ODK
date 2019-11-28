import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { adminPaths } from './admin-paths';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from '../../../../../src/app/services/chapters/chapter-admin.service';
import { RouteGuardService } from 'src/app/routing/route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterAdminGuardService extends RouteGuardService {

  constructor(router: Router, 
    private chapterService: ChapterAdminService
  ) { 
    super(router);
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const name: string = route.paramMap.get(adminPaths.chapter.params.chapter);
    return this.chapterService.getChapter(name).pipe(
      tap((chapter: Chapter) => this.chapterService.setActiveChapter(chapter)),
      map((chapter: Chapter) => !!chapter)
    );
  }
}
