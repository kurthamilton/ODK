import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { Member } from 'src/app/core/members/member';
import { MemberAdminService } from 'src/app/services/members/member-admin.service';
import { MemberSubscription } from 'src/app/core/members/member-subscription';
import { SubscriptionType } from 'src/app/core/account/subscription-type';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-member-subscription',
  templateUrl: './member-subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberSubscriptionComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private datePipe: DatePipe,
    private memberAdminService: MemberAdminService
  ) {     
  }

  @Input() member: Member;

  form: FormViewModel;

  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    expiryDate: TextInputFormControlViewModel;
    type: DropDownFormControlViewModel;
  };
  private subscription: MemberSubscription;

  ngOnChanges(): void {
    if (!this.member) {
      return;
    }

    this.memberAdminService.getMemberSubscription(this.member.id).subscribe((subscription: MemberSubscription) => {
      this.subscription = subscription;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.subscription.expiryDate = new Date(this.formControls.expiryDate.value);
    this.subscription.type = <SubscriptionType>parseInt(this.formControls.type.value, 10);
    this.memberAdminService.updateMemberSubscription(this.subscription).subscribe(() => {
      this.formCallback.next(true);
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.formControls = {
      expiryDate: new TextInputFormControlViewModel({
        id: 'expiry-date',
        label: {
          text: 'Expiry date'
        },
        inputType: 'date',
        value: this.datePipe.transform(this.subscription.expiryDate, 'yyyy-MM-dd')
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
      ]
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
