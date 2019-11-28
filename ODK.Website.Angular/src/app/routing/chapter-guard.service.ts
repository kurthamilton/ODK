import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';

import { Observable, of } from 'rxjs';
import { map, tap, switchMap } from 'rxjs/operators';

import { appPaths } from './app-paths';
import { Chapter } from '../core/chapters/chapter';
import { ChapterService } from '../services/chapters/chapter.service';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterGuardService extends RouteGuardService {

  constructor(router: Router, 
    private chapterService: ChapterService
  ) { 
    super(router);
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    const name: string = route.paramMap.get(appPaths.chapter.params.chapter);
    return this.chapterService.getChapter(name).pipe(
      switchMap((chapter: Chapter) => {
        this.chapterService.setActiveChapter(chapter);
        return of(chapter);
      }),
      map((chapter: Chapter) => !!chapter)
    );
  }
}
