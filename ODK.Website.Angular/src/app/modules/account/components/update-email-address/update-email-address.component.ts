import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { accountPaths } from '../../routing/account-paths';
import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { accountUrls } from '../../routing/account-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { RouteUtils } from 'src/app/utils/route-utils';
import { ServiceResult } from 'src/app/services/service-result';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-update-email-address',
  templateUrl: './update-email-address.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpdateEmailAddressComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterService: ChapterService,
    private accountService: AccountService
  ) {
  }

  breadcrumbs: MenuItem[];
  errorMessages: string[];
  form: FormViewModel;
  status: 'requested' | 'confirming' | 'updated' | 'confirmationError';

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    current: ReadOnlyFormControlViewModel;
    new: TextInputFormControlViewModel;
  };
  private profile: AccountProfile;

  ngOnInit(): void {
    this.chapter = this.chapterService.getActiveChapter();
    this.breadcrumbs = [
      { link: accountUrls.profile(this.chapter), text: 'Profile' }
    ];

    const token: string = RouteUtils.getQueryParam(this.route, accountPaths.updateEmailAddress.queryParams.token);
    if (token) {
      this.status = 'confirming';
      this.accountService.confirmEmailAddressUpdate(token).pipe(
        switchMap((result: ServiceResult<void>) => {
          if (!result.success) {
            this.status = 'confirmationError';
            this.errorMessages = result.messages;
          }
          return this.accountService.getProfile();
        })
      ).subscribe((profile: AccountProfile) => {
        this.router.navigateByUrl(accountUrls.updateEmailAddress(this.chapter));
        this.profile = profile;
        this.status = this.status !== 'confirmationError' ? 'updated' : this.status;
        this.buildForm();
        this.changeDetector.detectChanges();
      });
    } else {
      this.accountService.getProfile().subscribe((profile: AccountProfile) => {
        this.profile = profile;
        this.buildForm();
        this.changeDetector.detectChanges();
      });
    }
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onAlertClose(): void {
    this.status = null;
    this.changeDetector.detectChanges();
  }

  onFormSubmit(): void {
    this.accountService.requestEmailAddressUpdate(this.formControls.new.value).subscribe((result: ServiceResult<void>) => {
      this.status = 'requested';
      this.formCallback.next(result.messages);
      this.form = null;
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.formControls = {
      current: new ReadOnlyFormControlViewModel({
        id: 'current',
        label: {
          text: 'Current email address'
        },
        value: this.profile.emailAddress
      }),
      new: new TextInputFormControlViewModel({
        id: 'new',
        label: {
          text: 'New email address'
        },
        validation: {
          message: 'Invalid email address format',
          pattern: FormControlValidationPatterns.email,
          required: true
        }
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.current,
        this.formControls.new
      ]
    };
  }
}
