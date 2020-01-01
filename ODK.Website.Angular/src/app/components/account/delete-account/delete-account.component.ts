import { Component, ChangeDetectionStrategy, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { AccountService } from 'src/app/services/account/account.service';
import { appUrls } from 'src/app/routing/app-urls';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { NotificationService } from 'src/app/services/notifications/notification.service';

@Component({
  selector: 'app-delete-account',
  templateUrl: './delete-account.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeleteAccountComponent implements OnInit, OnDestroy {

  constructor(private accountService: AccountService,
    private router: Router,
    private authenticationService: AuthenticationService,
    private chapterService: ChapterService,
    private notificationService: NotificationService
  ) {     
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();

    this.breadcrumbs = [
      { link: appUrls.profile(this.chapter), text: 'Profile' }
    ];    

    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {    
    if (!confirm('Are you sure you want to delete your account?')) {
      this.formCallback.next(true);
      return;
    }

    this.accountService.deleteAccount().pipe(
      switchMap(() => this.authenticationService.logout())
    ).subscribe(() => {      
      this.formCallback.next(true);
      this.notificationService.schedule({
        message: 'Your account has been deleted',
        success: true
      });
      this.router.navigateByUrl(appUrls.chapter(this.chapter));
    });
  }

  private buildForm(): void {
    this.form = {
      buttons: [
        { text: 'Delete my account', type: 'danger' }
      ],
      callback: this.formCallback,
      controls: []
    };
  }
}
