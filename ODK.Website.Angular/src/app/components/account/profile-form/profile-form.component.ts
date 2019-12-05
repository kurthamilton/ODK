import { DatePipe } from '@angular/common';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { CheckBoxViewModel } from 'src/app/modules/forms/components/inputs/check-box/check-box.view-model';
import { DataType } from 'src/app/core/data-types/data-type';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlOptions } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-options';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormControlOptions } from 'src/app/modules/forms/components/form-control-options';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { MemberProperty } from 'src/app/core/members/member-property';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { TextAreaFormControlOptions } from 'src/app/modules/forms/components/inputs/text-area/text-area-form-control-options';
import { TextAreaViewModel } from 'src/app/modules/forms/components/inputs/text-area/text-area.view-model';
import { TextInputFormControlOptions } from 'src/app/modules/forms/components/inputs/text-input/text-input-form-control-options';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

@Component({
  selector: 'app-profile-form',
  templateUrl: './profile-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileFormComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private authenticationService: AuthenticationService,
    private accountService: AccountService,
    private chapterService: ChapterService,
    private datePipe: DatePipe
  ) {
  }

  @Output() updated: EventEmitter<boolean> = new EventEmitter<boolean>();

  form: FormViewModel;

  private chapterId: string;
  private chapterProperties: Map<string, ChapterProperty>;
  private chapterPropertyOptions: Map<string, ChapterPropertyOption[]>;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private profile: AccountProfile;

  private formControls: {
    emailAddress: ReadOnlyFormControlViewModel;
    emailOptIn: CheckBoxViewModel;
    firstName: TextInputViewModel;
    joined: ReadOnlyFormControlViewModel;
    lastName: TextInputViewModel;
    properties: FormControlViewModel[];
  };

  ngOnInit(): void {
    const authenticationToken: AuthenticationToken = this.authenticationService.getToken();
    this.chapterId = authenticationToken.chapterId;

    this.loadProfile();
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
    this.accountService.updateProfile(profile).subscribe(() => {
      this.updated.emit(true);
      this.formCallback.next(true);
      this.loadProfile();
    });
  }

  private buildForm(): void {
    this.formControls = {
      emailAddress: new ReadOnlyFormControlViewModel({
        id: 'emailAddresss',
        label: {
          text: 'Email'
        },
        value: this.profile.emailAddress
      }),
      emailOptIn: new CheckBoxViewModel({
        id: 'emailOptIn',
        label: {
          text: 'Receive emails',
          subtitle: 'Opt in to emails informing you of upcoming events'
        },
        value: this.profile.emailOptIn
      }),
      firstName: new TextInputViewModel({
        id: 'firstName',
        label: {
          text: 'First Name'
        },
        validation: {
          required: true
        },
        value: this.profile.firstName
      }),
      joined: new ReadOnlyFormControlViewModel({
        id: 'joined',
        label: {
          text: 'Date joined'
        },
        value: this.datePipe.transform(this.profile.joined, 'dd MMMM yyyy')
      }),
      lastName: new TextInputViewModel({
        id: 'lastName',
        label: {
          text: 'Last Name'
        },
        validation: {
          required: true
        },
        value: this.profile.lastName
      }),
      properties: this.profile.properties.map((x): FormControlViewModel => this.mapFormControl(x))
    }

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback.asObservable(),
      controls: [
        this.formControls.emailAddress,
        this.formControls.emailOptIn,
        this.formControls.firstName,
        this.formControls.lastName,
        ...this.formControls.properties,
        this.formControls.joined
      ]
    };
  }

  private loadProfile(): void {
    this.form = null;
    this.changeDetector.detectChanges();
    forkJoin([
      this.accountService.getProfile().pipe(
        tap((profile: AccountProfile) => this.profile = profile)
      ),
      this.chapterService.getChapterProperties(this.chapterId).pipe(
        tap((properties: ChapterProperty[]) => this.chapterProperties = ArrayUtils.toMap(properties, x => x.id))
      ),
      this.chapterService.getChapterPropertyOptions(this.chapterId).pipe(
        tap((options: ChapterPropertyOption[]) => this.chapterPropertyOptions = ArrayUtils.groupValues(options, x => x.chapterPropertyId, x => x))
      )
    ]).subscribe(() => {
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  private mapFormControl(property: MemberProperty): FormControlViewModel {
      const chapterProperty: ChapterProperty = this.chapterProperties.get(property.chapterPropertyId);

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
        textAreaOptions.value = property.value;
        return new TextAreaViewModel(textAreaOptions);
      }

      if (chapterProperty.dataType === DataType.DropDown) {
        const chapterPropertyOptions: ChapterPropertyOption[] = this.chapterPropertyOptions.get(chapterProperty.id) || [];
        const dropDownOptions = <DropDownFormControlOptions>options;
        dropDownOptions.options = [
          { default: true, text: 'Select...', value: '' },
          ...chapterPropertyOptions.map((x): DropDownFormControlOption => ({
            freeText: x.freeText,
            selected: property.value === x.value,
            text: x.value,
            value: x.value
          }))
        ];
        dropDownOptions.value = property.value;
        return new DropDownFormControlViewModel(dropDownOptions);
      }

      const textInputOptions = <TextInputFormControlOptions>options;
      textInputOptions.value = property.value;
      return new TextInputViewModel(textInputOptions);
  }
}
