import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot } from '@angular/router';

import { Observable, of } from 'rxjs';

import { ChapterService } from '../services/chapters/chapter.service';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class HomeGuardService extends RouteGuardService {

  constructor(private chapterService: ChapterService) {
    super(null);
  }

  hasAccess(route: ActivatedRouteSnapshot): Observable<boolean> {
    this.chapterService.setActiveChapter(null);
    return of(true);
  }
}
