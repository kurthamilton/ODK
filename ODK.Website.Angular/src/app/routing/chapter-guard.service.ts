import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, Router } from '@angular/router';

import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';

import { Chapter } from '../core/chapters/chapter';
import { ChapterService } from '../services/chapter/chapter.service';
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
    const name: string = route.paramMap.get('chapter');
    return this.chapterService.getChapter(name).pipe(
      tap((chapter: Chapter) => this.chapterService.setActiveChapter(chapter)),
      map((chapter: Chapter) => !!chapter)
    );
  }
}
