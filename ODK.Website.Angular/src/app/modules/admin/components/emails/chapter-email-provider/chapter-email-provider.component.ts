import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject, forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProviderSettings } from 'src/app/core/chapters/chapter-email-provider-settings';
import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
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
  private emailProviders: string[];
  private emailProviderSettings: ChapterEmailProviderSettings;
  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    apiKey: TextInputFormControlViewModel;
    emailProvider: DropDownFormControlViewModel;
    fromEmailAddress: TextInputFormControlViewModel;
    fromName: TextInputFormControlViewModel;
    smtpLogin: TextInputFormControlViewModel;
    smtpPassword: TextInputFormControlViewModel;
    smtpPort: TextInputFormControlViewModel;
    smtpServer: TextInputFormControlViewModel;
  };

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();

    forkJoin([
      this.emailAdminService.getChapterAdminEmailProviderSettings(this.chapter.id).pipe(
        tap((emailProviderSettings: ChapterEmailProviderSettings) => this.emailProviderSettings = emailProviderSettings)
      ),
      this.emailAdminService.getEmailProviders().pipe(
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
    this.emailProviderSettings.apiKey = this.formControls.apiKey.value;
    this.emailProviderSettings.emailProvider = this.formControls.emailProvider.value;
    this.emailProviderSettings.fromEmailAddress = this.formControls.fromEmailAddress.value;
    this.emailProviderSettings.fromName = this.formControls.fromName.value;
    this.emailProviderSettings.smtpLogin = this.formControls.smtpLogin.value;
    this.emailProviderSettings.smtpPassword = this.formControls.smtpPassword.value;
    this.emailProviderSettings.smtpPort = parseInt(this.formControls.smtpPort.value, 10);
    this.emailProviderSettings.smtpServer = this.formControls.smtpServer.value;

    this.emailAdminService.updateChapterAdminEmailProviderSettings(this.chapter.id, this.emailProviderSettings).subscribe(() => {
      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.formControls = {
      apiKey: new TextInputFormControlViewModel({
        id: 'api-key',
        label: {
          text: 'API Key'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.apiKey
      }),
      emailProvider: new DropDownFormControlViewModel({
        id: 'email-provider',
        label: {
          text: 'Provider'
        },
        options: this.emailProviders.map((provider: string): DropDownFormControlOption => ({
          text: provider,
          value: provider
        })),
        validation: {
          required: true
        },
        value: this.emailProviderSettings.emailProvider
      }),
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
      smtpPort: new TextInputFormControlViewModel({
        id: 'smtp-port',
        inputType: 'number',
        label: {
          text: 'SMTP Port'
        },
        validation: {
          required: true
        },
        value: this.emailProviderSettings.smtpPort.toString()
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
        this.formControls.emailProvider,
        this.formControls.fromEmailAddress,
        this.formControls.fromName,
        this.formControls.apiKey,
        this.formControls.smtpServer,
        this.formControls.smtpPort,
        this.formControls.smtpLogin,
        this.formControls.smtpPassword
      ]
    };
  }
}
