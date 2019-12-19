import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailSettings } from 'src/app/core/chapters/chapter-email-settings';
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
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    adminEmailAddress: TextInputFormControlViewModel;
    contactEmailAddress: TextInputFormControlViewModel;
    emailApiKey: TextInputFormControlViewModel;
    emailProvider: ReadOnlyFormControlViewModel;
    fromEmailAddress: TextInputFormControlViewModel;
    fromEmailName: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    this.chapterAdminService.getChapterAdminEmailSettings(this.chapter.id).subscribe((emailSettings: ChapterEmailSettings) => {
      this.emailSettings = emailSettings;
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
      emailProvider: new ReadOnlyFormControlViewModel({
        id: 'email-provider',
        label: {
          text: 'Provider'
        },
        value: this.emailSettings.emailProvider
      }),
      fromEmailAddress: new TextInputFormControlViewModel({
        id: 'from-email-address',
        label: {
          text: 'From email address'
        },
        validation: {
          required: true
        },
        value: this.emailSettings.fromEmailAddress
      }),
      fromEmailName: new TextInputFormControlViewModel({
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
        this.formControls.fromEmailAddress,
        this.formControls.fromEmailName,
        this.formControls.adminEmailAddress,
        this.formControls.contactEmailAddress,
        this.formControls.emailProvider,
        this.formControls.emailApiKey
      ]
    };
  }
}
