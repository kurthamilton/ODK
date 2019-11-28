import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

import { Observable, of } from 'rxjs';

import { AccountDetails } from '../core/account/account-details';
import { AuthenticationService } from '../services/authentication/authentication.service';
import { Chapter } from '../core/chapters/chapter';
import { ChapterService } from '../services/chapters/chapter.service';
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

    const chapter: Chapter = this.chapterService.getActiveChapter();
    return of(chapter && chapter.id === accountDetails.chapterId);
  }
}
