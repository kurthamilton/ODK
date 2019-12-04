import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges, OnDestroy } from '@angular/core';

import { forkJoin } from 'rxjs';
import { tap } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterPaymentSettings } from 'src/app/core/chapters/chapter-payment-settings';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';
import { Payment } from 'src/app/core/payments/payment';

@Component({
  selector: 'app-payment-button',
  templateUrl: './payment-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentButtonComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private countryService: CountryService,
  ) {
  }

  @Input() payment: Payment;

  country: Country;
  currencySymbol: string;
  paymentSettings: ChapterPaymentSettings;
  showForm = false;

  ngOnChanges(): void {
    if (!this.payment) {
      return;
    }

    const chapter: Chapter = this.chapterService.getActiveChapter();

    forkJoin([
      this.countryService.getCountry(chapter.countryId).pipe(
        tap((country: Country) => this.country = country)
      ),
      this.chapterService.getChapterPaymentSettings(chapter.id).pipe(
        tap((paymentSettings: ChapterPaymentSettings) => this.paymentSettings = paymentSettings)
      )
    ]).subscribe(() => {
      this.currencySymbol = this.country.currencySymbol;
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy() {}

  onPurchase(): void {
    this.showForm = true;
    this.changeDetector.detectChanges();
  }
}
