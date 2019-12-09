import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter, Input, OnChanges } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { DataType } from 'src/app/core/data-types/data-type';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlOptions } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-options';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormControlOptions } from 'src/app/modules/forms/components/form-control-options';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { MemberProperty } from 'src/app/core/members/member-property';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
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


  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private datePipe: DatePipe
  ) {
  }

  @Input() chapterId: string;
  @Input() formCallback: Observable<boolean | string[]>;
  @Input() profile: AccountProfile;
  @Output() formSubmit: EventEmitter<AccountProfile> = new EventEmitter<AccountProfile>();
  @Output() updated: EventEmitter<boolean> = new EventEmitter<boolean>();

  form: FormViewModel;

  private chapterProperties: ChapterProperty[];
  private chapterPropertyOptions: Map<string, ChapterPropertyOption[]>;
  private memberPropertyMap: Map<string, MemberProperty>;

  private formControls: {
    emailAddress: FormControlViewModel;
    emailOptIn: CheckBoxFormControlViewModel;
    firstName: TextInputFormControlViewModel;
    joined: ReadOnlyFormControlViewModel;
    lastName: TextInputFormControlViewModel;
    properties: FormControlViewModel[];
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
        tap((options: ChapterPropertyOption[]) => this.chapterPropertyOptions = ArrayUtils.groupValues(options, x => x.chapterPropertyId, x => x))
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
      emailOptIn: this.formControls.emailOptIn.value,
      firstName: this.formControls.firstName.value,
      joined: null,
      lastName: this.formControls.lastName.value,
      properties: this.formControls.properties.map((x): MemberProperty => ({
        chapterPropertyId: x.id,
        value: x.value
      }))
    };
    this.formSubmit.next(profile);
  }

  private buildForm(): void {
    this.formControls = {
      emailAddress: this.profile ? new ReadOnlyFormControlViewModel({
        id: 'emailAddresss',
        label: {
          text: 'Email'
        },
        value: this.profile.emailAddress
      }) : new TextInputFormControlViewModel({
        id: 'emailAddress',
        label: {
          text: 'Email'
        },
        validation: {
          required: true
        },
        value: ''
      }),
      emailOptIn: new CheckBoxFormControlViewModel({
        id: 'emailOptIn',
        label: {
          text: 'Receive emails',
          subtitle: 'Opt in to emails informing you of upcoming events'
        },
        value: this.profile ? this.profile.emailOptIn : true
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
        value: this.datePipe.transform(this.profile.joined, 'dd MMMM yyyy')
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
    }

    this.form = {
      buttonText: this.profile ? 'Update' : 'Create',
      callback: this.formCallback,
      controls: [
        this.formControls.emailAddress,
        this.formControls.emailOptIn,
        this.formControls.firstName,
        this.formControls.lastName,
        ...this.formControls.properties,        
      ]
    };

    if (this.formControls.joined) {
      this.form.controls.push(this.formControls.joined);
    }
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
        text: chapterProperty.name
      },
      validation: {
        required: chapterProperty.required
      }
    };

    if (chapterProperty.dataType === DataType.LongText) {
      const textAreaOptions = <TextAreaFormControlOptions>options;
      textAreaOptions.value = memberProperty ? memberProperty.value : '';
      return new TextAreaFormControlViewModel(textAreaOptions);
    }

    if (chapterProperty.dataType === DataType.DropDown) {
      const chapterPropertyOptions: ChapterPropertyOption[] = this.chapterPropertyOptions.get(chapterProperty.id) || [];
      const dropDownOptions = <DropDownFormControlOptions>options;
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

    const textInputOptions = <TextInputFormControlOptions>options;
    textInputOptions.value = memberProperty ? memberProperty.value : '';
    return new TextInputFormControlViewModel(textInputOptions);
  }
}
