import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailSettings } from 'src/app/core/chapters/chapter-email-settings';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-emails',
  templateUrl: './chapter-emails.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailsComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  emailSettings: ChapterEmailSettings;
  form: FormViewModel;

  private chapter: Chapter;
  private emailProviders: string[];
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    adminEmailAddress: TextInputFormControlViewModel;
    contactEmailAddress: TextInputFormControlViewModel;
    emailApiKey: TextInputFormControlViewModel;
    emailProvider: DropDownFormControlViewModel;
    fromEmailAddress: ReadOnlyFormControlViewModel;
    fromEmailName: ReadOnlyFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.chapterAdminService.getChapterAdminEmailSettings(this.chapter.id).pipe(
        tap((emailSettings: ChapterEmailSettings) => this.emailSettings = emailSettings)
      ),
      this.chapterAdminService.getEmailProviders().pipe(
        tap((providers: string[]) => this.emailProviders = providers)
      )  
    ]).subscribe(() => {
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.emailSettings.adminEmailAddress = this.formControls.adminEmailAddress.value;
    this.emailSettings.contactEmailAddress = this.formControls.contactEmailAddress.value;
    this.emailSettings.emailApiKey = this.formControls.emailApiKey.value;
    this.emailSettings.emailProvider = this.formControls.emailProvider.value;
    this.emailSettings.fromEmailAddress = this.formControls.fromEmailAddress.value;
    this.emailSettings.fromEmailName = this.formControls.fromEmailName.value;
    this.chapterAdminService.updateChapterEmailSettings(this.chapter.id, this.emailSettings).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      adminEmailAddress: new TextInputFormControlViewModel({
        id: 'admin-email-address',
        label: {
          text: 'Admin email address'          
        },
        validation: {
          required: true
        },
        value: this.emailSettings.adminEmailAddress
      }),
      contactEmailAddress: new TextInputFormControlViewModel({
        id: 'contact-email-address',
        label: {
          text: 'Contact email address'
        },
        validation: {
          required: true
        },
        value: this.emailSettings.contactEmailAddress
      }),
      emailApiKey: new TextInputFormControlViewModel({
        id: 'email-api-key',
        label: {
          text: 'Provider Api Key'
        },
        validation: {
          required: true
        },
        value: this.emailSettings.emailApiKey
      }),
      emailProvider: new DropDownFormControlViewModel({
        id: 'email-provider',
        label: {
          helpText: 'Event emails are sent via a third-party email provider',
          text: 'Provider'
        },
        options: this.emailProviders.map((provider: string): DropDownFormControlOption => ({
          text: provider,
          value: provider
        })),
        validation: {
          required: true
        },
        value: this.emailSettings.emailProvider
      }),
      fromEmailAddress: new ReadOnlyFormControlViewModel({
        id: 'from-email-address',
        label: {
          helpText: 'These settings are used by the third-party email provider. Email addresses need to be validated before they can be used.',
          text: 'From email address'
        },
        validation: {
          required: true
        },
        value: this.emailSettings.fromEmailAddress
      }),
      fromEmailName: new ReadOnlyFormControlViewModel({
        id: 'from-email-name',
        label: {
          text: 'From email name'
        },
        value: this.emailSettings.fromEmailName
      })
    };

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback,
      controls: [        
        this.formControls.adminEmailAddress,
        this.formControls.contactEmailAddress,
        this.formControls.emailProvider,
        this.formControls.emailApiKey,
        this.formControls.fromEmailAddress,
        this.formControls.fromEmailName
      ]
    };
  }
}
