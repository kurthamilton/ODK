import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProviderSettings } from 'src/app/core/emails/chapter-email-provider-settings';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { NumberInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/number-input-form-control/number-input-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-email-provider',
  templateUrl: './chapter-email-provider.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailProviderComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  form: FormViewModel;

  private chapter: Chapter;
  private emailProviderSettings: ChapterEmailProviderSettings;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    fromEmailAddress: TextInputFormControlViewModel;
    fromName: TextInputFormControlViewModel;
    smtpLogin: TextInputFormControlViewModel;
    smtpPassword: TextInputFormControlViewModel;
    smtpPort: NumberInputFormControlViewModel;
    smtpServer: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.emailAdminService.getChapterAdminEmailProviderSettings(this.chapter.id).pipe(
        tap((emailProviderSettings: ChapterEmailProviderSettings) => this.emailProviderSettings = emailProviderSettings)
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
    this.emailProviderSettings.fromEmailAddress = this.formControls.fromEmailAddress.value;
    this.emailProviderSettings.fromName = this.formControls.fromName.value;
    this.emailProviderSettings.smtpLogin = this.formControls.smtpLogin.value;
    this.emailProviderSettings.smtpPassword = this.formControls.smtpPassword.value;
    this.emailProviderSettings.smtpPort = this.formControls.smtpPort.value;
    this.emailProviderSettings.smtpServer = this.formControls.smtpServer.value;

    this.emailAdminService.updateChapterAdminEmailProviderSettings(this.chapter.id, this.emailProviderSettings).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      fromEmailAddress: new TextInputFormControlViewModel({
        id: 'from-email-address',
        label: {
          text: 'From Email Address'
        },
        validation: {
          pattern: FormControlValidationPatterns.email,
          required: true
        },
        value: this.emailProviderSettings.fromEmailAddress
      }),
      fromName: new TextInputFormControlViewModel({
        id: 'from-name',
        label: {
          text: 'From Name'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.fromName
      }),
      smtpLogin: new TextInputFormControlViewModel({
        id: 'smtp-login',
        label: {
          text: 'SMTP Login'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.smtpLogin
      }),
      smtpPassword: new TextInputFormControlViewModel({
        id: 'smtp-password',
        label: {
          text: 'SMTP Password'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.smtpPassword
      }),
      smtpPort: new NumberInputFormControlViewModel({
        id: 'smtp-port',
        label: {
          text: 'SMTP Port'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.smtpPort
      }),
      smtpServer: new TextInputFormControlViewModel({
        id: 'smtp-server',
        label: {
          text: 'SMTP Server'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.smtpServer
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.fromEmailAddress,
        this.formControls.fromName,
        this.formControls.smtpServer,
        this.formControls.smtpPort,
        this.formControls.smtpLogin,
        this.formControls.smtpPassword
      ]
    };
  }
}
