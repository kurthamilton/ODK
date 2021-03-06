import { formatDate } from '@angular/common';
import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter, Input, OnChanges, Inject, LOCALE_ID } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { appUrls } from 'src/app/routing/app-urls';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { ChapterMembershipSettings } from 'src/app/core/chapters/chapter-membership-settings';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { DataType } from 'src/app/core/data-types/data-type';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlOptions } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-options';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FileInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/file-input-form-control/file-input-form-control.view-model';
import { FormControlOptions } from 'src/app/modules/forms/components/form-control-options';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MemberProperty } from 'src/app/core/members/member-property';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { SocialMediaService } from 'src/app/services/social-media/social-media.service';
import { TextAreaFormControlOptions } from 'src/app/modules/forms/components/inputs/text-area-form-control/text-area-form-control-options';
import { TextAreaFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-area-form-control/text-area-form-control.view-model';
import { TextInputFormControlOptions } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control-options';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-profile-form',
  templateUrl: './profile-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileFormComponent implements OnChanges {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private socialMediaService: SocialMediaService,
    @Inject(LOCALE_ID) private locale: string
  ) {
  }

  @Input() chapterId: string;
  @Input() formCallback: Observable<boolean | string[]>;
  @Input() profile: AccountProfile;
  @Output() formSubmit: EventEmitter<AccountProfile> = new EventEmitter<AccountProfile>();
  @Output() imageUpload: EventEmitter<File> = new EventEmitter<File>();
  @Output() updated: EventEmitter<boolean> = new EventEmitter<boolean>();

  form: FormViewModel;

  private chapterMembershipSettings: ChapterMembershipSettings;
  private chapterProperties: ChapterProperty[];
  private chapterPropertyOptions: Map<string, ChapterPropertyOption[]>;
  private memberPropertyMap: Map<string, MemberProperty>;

  private formControls: {
    emailAddress: FormControlViewModel;
    firstName: TextInputFormControlViewModel;
    joined: ReadOnlyFormControlViewModel;
    lastName: TextInputFormControlViewModel;
    properties: FormControlViewModel[];
  };

  private joinFormControls: {
    emailOptIn: CheckBoxFormControlViewModel;
    image: FileInputFormControlViewModel;
    privacy: CheckBoxFormControlViewModel;
    subscription: CheckBoxFormControlViewModel;
    threeTenets: CheckBoxFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.chapterId) {
      return;
    }

    forkJoin([
      this.chapterService.getChapterProperties(this.chapterId).pipe(
        tap((properties: ChapterProperty[]) => this.chapterProperties = properties)
      ),
      this.chapterService.getChapterPropertyOptions(this.chapterId).pipe(
        tap((options: ChapterPropertyOption[]) => {
          this.chapterPropertyOptions = ArrayUtils.groupValues(options, x => x.chapterPropertyId, x => x);
        })
      ),
      this.chapterService.getChapterMembershipSettings(this.chapterId).pipe(
        tap((settings: ChapterMembershipSettings) => this.chapterMembershipSettings = settings)
      )
    ]).subscribe(() => {
      if (this.profile) {
        this.memberPropertyMap = ArrayUtils.toMap(this.profile.properties, x => x.chapterPropertyId);
      }

      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onFormSubmit(): void {
    const profile: AccountProfile = {
      emailAddress: this.formControls.emailAddress.value,
      emailOptIn: this.profile ? this.profile.emailOptIn : this.joinFormControls.emailOptIn.value,
      firstName: this.formControls.firstName.value,
      joined: null,
      lastName: this.formControls.lastName.value,
      properties: this.formControls.properties.map((x): MemberProperty => ({
        chapterPropertyId: x.id,
        value: x.value
      }))
    };

    if (this.joinFormControls && this.joinFormControls.image.value && this.joinFormControls.image.value.length >= 1) {
      this.imageUpload.emit(this.joinFormControls.image.value[0]);
    }

    this.formSubmit.emit(profile);
  }

  private buildForm(): void {
    this.formControls = {
      emailAddress: this.profile ? new ReadOnlyFormControlViewModel({
        id: 'emailAddress',
        label: {
          text: 'Email address'
        },
        value: this.profile.emailAddress
      }) : new TextInputFormControlViewModel({
        id: 'emailAddress',
        label: {
          text: 'Email address',
          subtitle: 'You will need access to this email to verify your account'
        },
        validation: {
          pattern: FormControlValidationPatterns.email,
          required: true
        },
        value: ''
      }),
      firstName: new TextInputFormControlViewModel({
        id: 'firstName',
        label: {
          text: 'First Name'
        },
        validation: {
          required: true
        },
        value: this.profile ? this.profile.firstName : ''
      }),
      joined: this.profile ? new ReadOnlyFormControlViewModel({
        id: 'joined',
        label: {
          text: 'Date joined'
        },
        value: formatDate(this.profile.joined, 'dd MMMM yyyy', this.locale)
      }) : null,
      lastName: new TextInputFormControlViewModel({
        id: 'lastName',
        label: {
          text: 'Last Name'
        },
        validation: {
          required: true
        },
        value: this.profile ? this.profile.lastName : ''
      }),
      properties: this.chapterProperties.map(x => this.mapFormControl(x))
    };

    if (!this.profile) {
      this.joinFormControls = {
        emailOptIn: this.profile ? null : new CheckBoxFormControlViewModel({
          id: 'emailOptIn',
          label: {
            helpText: 'We recommend you leave this on. Once you\'re set up, we\'ll only send you one email per event.',
            text: 'Receive event emails',
            subtitle: 'Opt in to emails informing you of upcoming events'
          },
          value: this.profile ? this.profile.emailOptIn : true
        }),
        image: new FileInputFormControlViewModel({
          fileType: 'image',
          id: 'image',
          label: {
            text: 'Picture'
          },
          validation: {
            required: true
          }
        }),
        privacy: new CheckBoxFormControlViewModel({
          id: 'privacy',
          label: {
            text: `I have read the <a href="${appUrls.privacy}" target="_blank">privacy policy</a>`,
            textIsHtml: true
          },
          validation: {
            message: 'Required',
            pattern: '^true$',
            required: true
          },
          value: false
        }),
        subscription: new CheckBoxFormControlViewModel({
          id: 'subscription',
          label: {
            text: `If I choose to continue attending after my free ${this.chapterMembershipSettings.trialPeriodMonths} month trial period has expired, I agree to purchase a subscription.`
          },
          validation: {
            message: 'Required',
            pattern: '^true$',
            required: true
          },
          value: false
        }),
        threeTenets: new CheckBoxFormControlViewModel({
          id: 'three-tenets',
          label: {
            text: 'I abide by the three <a href="/#tenets" target="_blank">tenets</a> of Drunken Knithood',
            textIsHtml: true
          },
          validation: {
            message: 'Required',
            pattern: '^true$',
            required: true
          },
          value: false
        })
      };
    }

    const controls: FormControlViewModel[] = [
      this.formControls.emailAddress
    ];

    if (this.joinFormControls) {
      controls.push(
        this.joinFormControls.emailOptIn
      );
    }

    controls.push(...[
      this.formControls.firstName,
      this.formControls.lastName,
      ...this.formControls.properties,
    ]);

    if (this.formControls.joined) {
      controls.push(this.formControls.joined);
    }

    if (this.joinFormControls) {
      controls.push(...[
        this.joinFormControls.image,
        this.joinFormControls.threeTenets,
        this.joinFormControls.privacy,
        this.joinFormControls.subscription
      ]);
    }

    this.form = {
      buttons: [
        { text: this.profile ? 'Update' : 'Create' }
      ],
      callback: this.formCallback,
      controls
    };
  }

  private mapFormControl(chapterProperty: ChapterProperty): FormControlViewModel {
    const memberProperty: MemberProperty = this.memberPropertyMap
      ? this.memberPropertyMap.get(chapterProperty.id)
      : null;

    const options: FormControlOptions = {
      id: chapterProperty.id,
      label: {
        helpText: chapterProperty.helpText,
        subtitle: chapterProperty.subtitle,
        text: chapterProperty.label
      },
      validation: {
        required: chapterProperty.required
      }
    };

    if (chapterProperty.dataType === DataType.LongText) {
      const textAreaOptions = options as TextAreaFormControlOptions;
      textAreaOptions.value = memberProperty ? memberProperty.value : '';
      return new TextAreaFormControlViewModel(textAreaOptions);
    }

    if (chapterProperty.dataType === DataType.DropDown) {
      const chapterPropertyOptions: ChapterPropertyOption[] = this.chapterPropertyOptions.get(chapterProperty.id) || [];
      if (chapterPropertyOptions.length > 0) {
        const dropDownOptions = options as DropDownFormControlOptions;
        dropDownOptions.options = [
          { default: true, text: 'Select...', value: '' },
          ...chapterPropertyOptions.map((x): DropDownFormControlOption => ({
            freeText: x.freeText,
            selected: memberProperty ? memberProperty.value === x.value : false,
            text: x.value,
            value: x.value
          }))
        ];
        dropDownOptions.value = memberProperty ? memberProperty.value : '';
        return new DropDownFormControlViewModel(dropDownOptions);
      }
    }

    const textInputOptions = options as TextInputFormControlOptions;
    textInputOptions.prefix = chapterProperty.name === 'facebook' ? {
      icon: 'fa-facebook-f',
      text: this.socialMediaService.getFacebookAccountLink('')
    } : null;
    textInputOptions.value = memberProperty ? memberProperty.value : '';
    return new TextInputFormControlViewModel(textInputOptions);
  }
}
