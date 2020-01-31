import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminMembershipSettings } from 'src/app/core/chapters/chapter-admin-membership-settings';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { NumberInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/number-input-form-control/number-input-form-control.view-model';
import { ServiceResult } from 'src/app/services/service-result';

@Component({
  selector: 'app-membership-settings',
  templateUrl: './membership-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MembershipSettingsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {
  }

  form: FormViewModel;

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    membershipDisabledAfterDaysExpired: NumberInputFormControlViewModel;
    trialPeriodMonths: NumberInputFormControlViewModel;
  };
  private membershipSettings: ChapterAdminMembershipSettings;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterAdminMembershipSettings(this.chapter.id).subscribe((settings: ChapterAdminMembershipSettings) => {
      this.membershipSettings = settings;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.membershipSettings.membershipDisabledAfterDaysExpired = this.formControls.membershipDisabledAfterDaysExpired.value;
    this.membershipSettings.trialPeriodMonths = this.formControls.trialPeriodMonths.value;

    this.chapterAdminService.updateChapterAdminMembershipSettings(this.chapter.id, this.membershipSettings).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
    });
  }

  private buildForm(): void {
    this.formControls = {
      membershipDisabledAfterDaysExpired: new NumberInputFormControlViewModel({
        id: 'membership-disabled-after-days-expired',
        label: {
          text: 'Membership disabled after',
          helpText: 'Members will be able to login, but not see or respond to events. They will still receive event emails.',
          subtitle: 'How many days after subscription expiry a member has access to events'
        },
        min: 0,
        validation: {
          required: true
        },
        value: this.membershipSettings.membershipDisabledAfterDaysExpired
      }),
      trialPeriodMonths: new NumberInputFormControlViewModel({
        id: 'trial-period-months',
        label: {
          text: 'Trial period (months)'
        },
        min: 1,
        validation: {
          required: true
        },
        value: this.membershipSettings.trialPeriodMonths
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.trialPeriodMonths,
        this.formControls.membershipDisabledAfterDaysExpired
      ],
      messages: {
        success: 'Updated'
      }
    };
  }
}
