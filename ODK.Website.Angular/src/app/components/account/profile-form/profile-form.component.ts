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
import { FormControlType } from 'src/app/modules/forms/components/form-control-type';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { MemberProperty } from 'src/app/core/members/member-property';

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

  form: FormViewModel;

  private chapterId: string;
  private chapterProperties: Map<string, ChapterProperty>;
  private chapterPropertyOptions: Map<string, ChapterPropertyOption[]>;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private profile: AccountProfile;

  private formControls: {
    emailAddress: FormControlViewModel;
    emailOptIn: FormControlViewModel;
    firstName: FormControlViewModel;
    joined: FormControlViewModel;
    lastName: FormControlViewModel;
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
      emailOptIn: this.formControls.emailOptIn.value === 'true',
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
      emailAddress: {
        id: 'emailAddresss',
        label: {
          text: 'Email'
        },
        validators: {
          required: true
        },
        value: this.profile.emailAddress
      },
      emailOptIn: {
        id: 'emailOptIn',
        label: {
          text: 'Receive emails',
          subtitle: 'Opt in to emails informing you of upcoming events'
        },
        type: 'checkbox',
        value: this.profile.emailOptIn ? 'true' : 'false'
      },
      firstName: {
        id: 'firstName',
        label: {
          text: 'First Name'
        },
        validators: {
          required: true
        },
        value: this.profile.firstName
      },
      joined: {
        id: 'joined',
        label: {
          text: 'Date joined'
        },
        type: 'readonly',
        value: this.datePipe.transform(this.profile.joined, 'dd MMMM yyyy')
      },
      lastName: {
        id: 'lastName',
        label: {
          text: 'Last Name'
        },
        validators: {
          required: true
        },
        value: this.profile.lastName
      },
      properties: this.profile.properties.map((x): FormControlViewModel => this.mapFormControl(x))
    }

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback.asObservable(),
      formControls: [
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

      const formControl: FormControlViewModel = {        
        id: property.chapterPropertyId,
        label: {
          helpText: chapterProperty.helpText,
          subtitle: chapterProperty.subtitle,
          text: chapterProperty.name
        },        
        type: this.mapFormControlType(chapterProperty.dataType),
        value: property.value,
        validators: {
          required: chapterProperty.required
        }
      };

      if (chapterProperty.dataType === DataType.DropDown) {
        const chapterPropertyOptions: ChapterPropertyOption[] = this.chapterPropertyOptions.get(chapterProperty.id) || [];
        formControl.dropDown = {
          default: 'Select...',
          freeTextOption: chapterPropertyOptions.find(x => x.freeText).value,
          options: chapterPropertyOptions.map(x => x.value)
        };
      }

      return formControl;
  }

  private mapFormControlType(dataType: DataType): FormControlType {
    switch (dataType) {
      case DataType.DropDown:
        return 'dropdown';
      case DataType.LongText:
        return 'textarea';
      default:
        return 'text';
    }
  }
}
