import { DatePipe } from '@angular/common';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { AccountProfile } from 'src/app/core/account/account-profile';
import { AccountService } from 'src/app/services/account/account.service';
import { ArrayUtils } from 'src/app/utils/array-utils';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { AuthenticationToken } from 'src/app/core/authentication/authentication-token';
import { ChapterPropertyOption } from 'src/app/core/chapters/chapter-property-option';
import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { DataType } from 'src/app/core/data-types/data-type';
import { MemberProperty } from 'src/app/core/members/member-property';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { CheckBoxViewModel } from 'src/app/modules/forms/components/inputs/check-box/check-box.view-model';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';
import { DynamicFormControlViewModel } from 'src/app/modules/forms/components/dynamic-form-control.view-model';
import { TextAreaViewModel } from 'src/app/modules/forms/components/inputs/text-area/text-area.view-model';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { DynamicFormViewModel } from 'src/app/modules/forms/components/dynamic-form.view-model';

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

  form: DynamicFormViewModel;

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
    properties: DynamicFormControlViewModel[];
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
        validators: {
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
        validators: {
          required: true
        },
        value: this.profile.lastName
      }),
      properties: this.profile.properties.map((x): DynamicFormControlViewModel => this.mapFormControl(x))
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

  private mapFormControl(property: MemberProperty): DynamicFormControlViewModel {
      const chapterProperty: ChapterProperty = this.chapterProperties.get(property.chapterPropertyId);

      const options: any = {
        id: chapterProperty.id,
        label: {
          helpText: chapterProperty.helpText,
          subtitle: chapterProperty.subtitle,
          text: chapterProperty.name  
        },
        validators: {
          required: chapterProperty.required
        }
      };

      if (chapterProperty.dataType === DataType.LongText) {
        return new TextAreaViewModel(options);
      }
      
      if (chapterProperty.dataType === DataType.DropDown) {
        const chapterPropertyOptions: ChapterPropertyOption[] = this.chapterPropertyOptions.get(chapterProperty.id) || [];
        options.options = {
          default: 'Select...',
          freeTextOption: chapterPropertyOptions.find(x => x.freeText).value,
          options: chapterPropertyOptions.map(x => x.value)
        };
        return new DropDownFormControlViewModel(options);
      }
      
      return new TextInputViewModel(options);
  }
}
