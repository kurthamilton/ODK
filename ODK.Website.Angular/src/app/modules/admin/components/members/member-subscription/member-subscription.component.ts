import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';

import { Subject } from 'rxjs';

import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { DateInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/date-input-form-control/date-input-form-control.view-model';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { ServiceResult } from 'src/app/services/service-result';
import { SubscriptionType } from 'src/app/core/account/subscription-type';

@Component({
  selector: 'app-member-subscription',
  templateUrl: './member-subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberSubscriptionComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private router: Router,
    private memberAdminService: MemberAdminService,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  form: FormViewModel;
  member: Member;

  private chapter: Chapter;
  private formCallback: Subject<string[]> = new Subject<string[]>();
  private formControls: {
    expiryDate: DateInputFormControlViewModel;
    type: DropDownFormControlViewModel;
  };
  private subscription: MemberSubscription;

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.member = this.memberAdminService.getActiveMember();
    
    this.memberAdminService.getMemberSubscription(this.member.id).subscribe((subscription: MemberSubscription) => {
      this.subscription = subscription;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onDeleteMember(): void {
    if (!confirm('Are you sure you want to delete this member and all associated data?')) {
      return;
    }

    this.memberAdminService.deleteMember(this.member.id).subscribe(() => {
      this.router.navigateByUrl(adminUrls.members(this.chapter));
    });
  }

  onFormSubmit(): void {
    this.subscription.expiryDate = this.formControls.expiryDate.value;
    this.subscription.type = <SubscriptionType>parseInt(this.formControls.type.value, 10);
    this.memberAdminService.updateMemberSubscription(this.subscription).subscribe((result: ServiceResult<void>) => {
      this.formCallback.next(result.messages);
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.formControls = {
      expiryDate: new DateInputFormControlViewModel({
        id: 'expiry-date',
        label: {
          text: 'Expiry date'
        },
        value: this.subscription.expiryDate
      }),
      type: new DropDownFormControlViewModel({
        id: 'type',
        label: {
          text: 'Type'
        },
        options: [
          this.createTypeOption(SubscriptionType.Trial),
          this.createTypeOption(SubscriptionType.Full),
          this.createTypeOption(SubscriptionType.Partial),
          this.createTypeOption(SubscriptionType.Alum)
        ],
        validation: {
          required: true
        },
        value: this.subscription.type.toString()
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.type,
        this.formControls.expiryDate
      ],
      messages: {
        success: 'Member subscription updated'
      }
    };
  }

  private createTypeOption(type: SubscriptionType): DropDownFormControlOption {
    return { 
      text: SubscriptionType[type], 
      value: type.toString(), 
      selected: this.subscription.type === type
    };
  }
}
