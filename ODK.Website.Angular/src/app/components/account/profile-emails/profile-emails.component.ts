import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { appUrls } from 'src/app/routing/app-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';

@Component({
  selector: 'app-profile-emails',
  templateUrl: './profile-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private accountService: AccountService
  ) {     
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;
  profile: AccountProfile;

  private chapter: Chapter;
  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: appUrls.profile(this.chapter), text: 'Profile' }
    ];
    
    this.accountService.getProfile().subscribe((profile: AccountProfile) => {
      this.profile = profile;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    const optIn: boolean = !this.profile.emailOptIn;
    this.accountService.updateEmailOptIn(optIn).subscribe(() => {
      this.profile.emailOptIn = optIn;
      this.buildForm();
      this.formCallback.next(true);
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.form = {
      buttons: [
        { 
          text: this.profile.emailOptIn ? 'Unsubscribe' : 'Opt in',
          type: this.profile.emailOptIn ? 'danger' : 'success'
        }
      ],
      callback: this.formCallback,
      controls: []
    };
  }
}
