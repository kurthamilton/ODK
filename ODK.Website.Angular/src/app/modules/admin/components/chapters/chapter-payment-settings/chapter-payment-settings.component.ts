import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { Subject } from 'rxjs';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterAdminPaymentSettings } from 'src/app/core/chapters/chapter-admin-payment-settings';
import { ChapterAdminService } from 'src/app/services/chapters/chapter-admin.service';
import { FormViewModel } from 'src/app/modules/forms/components/form.view-model';
import { ReadOnlyFormControlViewModel } from 'src/app/modules/forms/components/inputs/read-only-form-control/read-only-form-control.view-model';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

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

  private chapter: Chapter;
  private controls: {
    apiPublicKey: TextInputViewModel,
    apiSecretKey: TextInputViewModel,
    provider: ReadOnlyFormControlViewModel
  };
  private formCallback: Subject<boolean> = new Subject<boolean>();

  ngOnInit(): void {
    this.chapter = this.chapterAdminService.getActiveChapter();
    this.chapterAdminService.getChapterAdminPaymentSettings(this.chapter.id).subscribe((paymentSettings: ChapterAdminPaymentSettings) => {
      this.paymentSettings = paymentSettings;
      this.buildForm();
      this.changeDetector.detectChanges();
    });
  }

  onFormSubmit(): void {
    this.paymentSettings.apiPublicKey = this.controls.apiPublicKey.value;
    this.paymentSettings.apiSecretKey = this.controls.apiSecretKey.value;

    this.chapterAdminService.updateChapterAdminPaymentSettings(this.chapter.id, this.paymentSettings).subscribe((paymentSettings: ChapterAdminPaymentSettings) => {
      this.paymentSettings = paymentSettings;
      this.buildForm();
      this.changeDetector.detectChanges();

      this.formCallback.next(true);
    });
  }

  private buildForm(): void {
    this.controls = {
      apiPublicKey: new TextInputViewModel({
        id: 'apipublickey',
        label: {
          text: 'Public key'
        },
        value: this.paymentSettings.apiPublicKey
      }),
      apiSecretKey: new TextInputViewModel({
        id: 'apisecretkey',
        label: {
          text: 'Secret key'
        },
        value: this.paymentSettings.apiSecretKey
      }),
      provider: new ReadOnlyFormControlViewModel({
        id: 'provider',
        label: {
          text: 'Provider'
        },
        value: this.paymentSettings.provider
      })
    };

    this.form = {
      buttonText: 'Update',
      callback: this.formCallback,
      controls: [
        this.controls.provider,
        this.controls.apiPublicKey,
        this.controls.apiSecretKey
      ]
    }
  }
}
