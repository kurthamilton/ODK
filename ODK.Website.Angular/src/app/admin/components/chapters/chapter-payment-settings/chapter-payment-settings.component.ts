import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { FormViewModel } from 'src/app/components/forms/form.view-model';
import { Subject } from 'rxjs';
import { FormControlViewModel } from 'src/app/components/forms/form-control.view-model';

@Component({
  selector: 'app-chapter-payment-settings',
  templateUrl: './chapter-payment-settings.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPaymentSettingsComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterAdminService: ChapterAdminService
  ) {     
  }

  form: FormViewModel;
  paymentSettings: ChapterAdminPaymentSettings;

  private controls: {
    apiPublicKey: FormControlViewModel,
    apiSecretKey: FormControlViewModel,
    provider: FormControlViewModel    
  };
  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    const chapter: Chapter = this.chapterAdminService.getActiveChapter();
    this.chapterAdminService.getChapterAdminPaymentSettings(chapter.id).subscribe((paymentSettings: ChapterAdminPaymentSettings) => {
      this.paymentSettings = paymentSettings;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  private buildForm(): void {
    this.controls = {
      apiPublicKey: {
        id: 'apipublickey',
        label: 'Public key',
        value: this.paymentSettings.apiPublicKey
      },
      apiSecretKey: {
        id: 'apisecretkey',
        label: 'Secret key',
        value: this.paymentSettings.apiSecretKey
      },
      provider: {
        id: 'provider',
        label: 'Provider',
        type: 'readonly',
        value: this.paymentSettings.provider
      }
    };
    
    this.form = {
      buttonText: 'Update',
      callback: this.formCallback,
      formControls: [
        this.controls.provider,
        this.controls.apiPublicKey,
        this.controls.apiSecretKey
      ]
    }
  }
}