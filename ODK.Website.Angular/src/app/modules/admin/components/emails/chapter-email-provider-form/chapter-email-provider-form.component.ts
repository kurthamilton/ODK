import { Component, ChangeDetectionStrategy, Input, Output, EventEmitter, OnChanges } from '@angular/core';

import { Observable } from 'rxjs';

import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { NumberInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/number-input-form-control/number-input-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-email-provider-form',
  templateUrl: './chapter-email-provider-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailProviderFormComponent implements OnChanges {

  @Input() buttonText: string;  
  @Input() formCallback: Observable<boolean | string[]>;
  @Input() provider: ChapterEmailProvider;
  @Output() formSubmit: EventEmitter<ChapterEmailProvider> = new EventEmitter<ChapterEmailProvider>();

  form: FormViewModel;

  private formControls: {
    batchSize: NumberInputFormControlViewModel;
    dailyLimit: NumberInputFormControlViewModel;
    fromEmailAddress: TextInputFormControlViewModel;
    fromName: TextInputFormControlViewModel;
    smtpLogin: TextInputFormControlViewModel;
    smtpPassword: TextInputFormControlViewModel;
    smtpPort: NumberInputFormControlViewModel;
    smtpServer: TextInputFormControlViewModel;
  };
  
  ngOnChanges(): void {
    if (!this.provider) {
      return;
    }

    this.buildForm();
  }

  onFormSubmit(): void {
    this.provider.batchSize = this.formControls.batchSize.value;
    this.provider.dailyLimit = this.formControls.dailyLimit.value;
    this.provider.fromEmailAddress = this.formControls.fromEmailAddress.value;
    this.provider.fromName = this.formControls.fromName.value;
    this.provider.smtpLogin = this.formControls.smtpLogin.value;
    this.provider.smtpPassword = this.formControls.smtpPassword.value;
    this.provider.smtpPort = this.formControls.smtpPort.value;
    this.provider.smtpServer = this.formControls.smtpServer.value;

    this.formSubmit.emit(this.provider);
  }

  private buildForm(): void {
    this.formControls = {
      batchSize: new NumberInputFormControlViewModel({
        id: 'batch-size',
        label: {
          text: 'Batch size'
        },
        min: 0,
        value: this.provider.batchSize
      }),
      dailyLimit: new NumberInputFormControlViewModel({
        id: 'daily-limit',
        label: {
          text: 'Daily limit'
        },
        min: 1,
        validation: {
          required: true
        },
        value: this.provider.dailyLimit
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
        value: this.provider.fromEmailAddress
      }),
      fromName: new TextInputFormControlViewModel({
        id: 'from-name',
        label: {
          text: 'From Name'
        },
        validation: {
          required: true
        },
        value: this.provider.fromName
      }),
      smtpLogin: new TextInputFormControlViewModel({
        id: 'smtp-login',
        label: {
          text: 'SMTP Login'
        },
        validation: {
          required: true
        },
        value: this.provider.smtpLogin
      }),
      smtpPassword: new TextInputFormControlViewModel({
        id: 'smtp-password',
        label: {
          text: 'SMTP Password'
        },
        validation: {
          required: true
        },
        value: this.provider.smtpPassword
      }),
      smtpPort: new NumberInputFormControlViewModel({
        id: 'smtp-port',
        label: {
          text: 'SMTP Port'
        },
        validation: {
          required: true
        },
        value: this.provider.smtpPort
      }),
      smtpServer: new TextInputFormControlViewModel({
        id: 'smtp-server',
        label: {
          text: 'SMTP Server'
        },
        validation: {
          required: true
        },
        value: this.provider.smtpServer
      })
    };

    this.form = {
      buttons: [
        { text: this.buttonText }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.fromEmailAddress,
        this.formControls.fromName,
        this.formControls.smtpServer,
        this.formControls.smtpPort,
        this.formControls.smtpLogin,
        this.formControls.smtpPassword,
        this.formControls.batchSize,
        this.formControls.dailyLimit
      ]
    };
  }
}
