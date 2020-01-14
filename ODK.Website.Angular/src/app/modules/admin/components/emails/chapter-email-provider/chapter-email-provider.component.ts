import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';

import { Subject } from 'rxjs';

import { adminPaths } from '../../../routing/admin-paths';
import { adminUrls } from '../../../routing/admin-urls';
import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { ChapterEmailProvider } from 'src/app/core/emails/chapter-email-provider';
import { EmailAdminService } from 'src/app/services/emails/email-admin.service';
import { FormControlValidationPatterns } from 'src/app/modules/forms/components/form-control-validation/form-control-validation-patterns';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MenuItem } from 'src/app/core/menus/menu-item';
import { NumberInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/number-input-form-control/number-input-form-control.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-email-provider',
  templateUrl: './chapter-email-provider.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterEmailProviderComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    private chapterAdminService: ChapterAdminService,
    private emailAdminService: EmailAdminService
  ) {
  }

  breadcrumbs: MenuItem[];
  form: FormViewModel;
  provider: ChapterEmailProvider;

  private chapter: Chapter;  
  private formCallback: Subject<boolean> = new Subject<boolean>();
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

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    
    const chapterEmailProviderId: string = this.route.snapshot.paramMap.get(adminPaths.emails.emailProviders.emailProvider.params.id);
    this.emailAdminService.getChapterAdminEmailProvider(this.chapter.id, chapterEmailProviderId).subscribe((provider: ChapterEmailProvider) => {
      if (!provider) {
        this.router.navigateByUrl(adminUrls.emailProviders(this.chapter));
        return;
      }

      this.breadcrumbs = [
        { link: adminUrls.emailProviders(this.chapter), text: 'Providers' }
      ];

      this.provider = provider;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormSubmit(): void {
    this.provider.fromEmailAddress = this.formControls.fromEmailAddress.value;
    this.provider.fromName = this.formControls.fromName.value;
    this.provider.smtpLogin = this.formControls.smtpLogin.value;
    this.provider.smtpPassword = this.formControls.smtpPassword.value;
    this.provider.smtpPort = this.formControls.smtpPort.value;
    this.provider.smtpServer = this.formControls.smtpServer.value;

    this.emailAdminService.updateChapterAdminEmailProvider(this.chapter.id, this.provider).subscribe(() => {
      this.formCallback.next(true);
    });
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
        { text: 'Update' }
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
