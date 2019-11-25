import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';

import { AccountDetails } from '../core/account/account-details';
import { AuthenticationService } from '../services/authentication/authentication.service';
import { Chapter } from '../core/chapters/chapter';
import { ChapterService } from '../services/chapter/chapter.service';
import { RouteGuardService } from './route-guard.service';

@Injectable({
  providedIn: 'root'
})
export class ChapterMemberGuardService extends RouteGuardService {

  constructor(router: Router,
    private chapterService: ChapterService,
    private authenticationService: AuthenticationService
  ) { 
    super(router);
  }

  hasAccess(): Observable<boolean> {
    const accountDetails: AccountDetails = this.authenticationService.getAccountDetails();
    if (!accountDetails) {
      return of(false);
    }

    return this.chapterService.getActiveChapter().pipe(
      map((chapter: Chapter) => chapter && chapter.id === accountDetails.chapterId)
    );
  }
}
