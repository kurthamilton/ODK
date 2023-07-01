import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges, OnDestroy, Output, EventEmitter } from '@angular/core';

import { forkJoin, Observable } from 'rxjs';
import { tap, takeUntil } from 'rxjs/operators';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterPaymentSettings } from 'src/app/core/chapters/chapter-payment-settings';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';
import { Payment } from 'src/app/core/payments/payment';

@Component({
  selector: 'app-payment-button',
  templateUrl: './payment-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentButtonComponent implements OnChanges, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private countryService: CountryService,
  ) {
  }

  @Input() close: Observable<void>;
  @Input() payment: Payment;
  @Input() title: string;
  @Output() cardSubmit: EventEmitter<string> = new EventEmitter<string>();

  country: Country;
  currencySymbol: string;
  paymentSettings: ChapterPaymentSettings;
  showForm = false;

  ngOnChanges(): void {
    if (!this.payment) {
      return;
    }

    if (this.close) {
      this.close.pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe(() => {
        this.showForm = false;
        this.changeDetector.detectChanges();
      });
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

  onCardSubmit(token: string): void {
    this.cardSubmit.emit(token);
  }

  onFormClose(): void {
    this.showForm = false;
    this.changeDetector.detectChanges();
  }

  onPurchase(): void {
    this.showForm = true;
    this.changeDetector.detectChanges();
  }
}
