import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { forkJoin, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { accountUrls } from '../../routing/account-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private accountService: AccountService
  ) {
  }

  chapterId: string;
  formCallback: Subject<boolean> = new Subject<boolean>();
  profile: AccountProfile;
  links: {
    changePassword: string;
    delete: string;
    emails: string;
    subscription: string;
    updateEmailAddress: string;
  };

  successMessage: string;

  private chapter: Chapter;

  ngOnInit(): void {
    const authenticationToken: AuthenticationToken = this.authenticationService.getToken();
    this.chapterId = authenticationToken.chapterId;

    forkJoin([
      this.accountService.getProfile().pipe(
        tap((profile: AccountProfile) => this.profile = profile)
      ),
      this.chapterService.getChapterById(this.chapterId).pipe(
        tap((chapter: Chapter) => this.chapter = chapter)
      )
    ]).subscribe(() => {
      this.links = {
        changePassword: accountUrls.password.change(this.chapter),
        delete: accountUrls.delete(this.chapter),
        emails: accountUrls.emails(this.chapter),
        subscription: accountUrls.subscription(this.chapter),
        updateEmailAddress: accountUrls.updateEmailAddress(this.chapter)
      };
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(profile: AccountProfile): void {
    this.accountService.updateProfile(profile).subscribe(() => {
      this.formCallback.next(true);
      this.successMessage = 'Your profile has been updated';
      this.changeDetector.detectChanges();
      window.scrollTo(0, 0);
    });
  }

  onAlertClose(): void {
    this.successMessage = null;
    this.changeDetector.detectChanges();
  }
}
