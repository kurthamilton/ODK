import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, OnChanges } from '@angular/core';

import { Chapter } from 'src/app/core/chapters/chapter';
import { ChapterService } from 'src/app/services/chapters/chapter.service';
import { Country } from 'src/app/core/countries/country';
import { CountryService } from 'src/app/services/countries/country.service';
import { Payment } from 'src/app/core/payments/payment';

@Component({
  selector: 'app-payment-button',
  templateUrl: './payment-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaymentButtonComponent implements OnChanges {

  constructor(private changeDetector: ChangeDetectorRef,
    private chapterService: ChapterService,
    private countryService: CountryService
  ) {     
  }

  @Input() payment: Payment;

  currencySymbol: string;

  ngOnChanges(): void {
    if (!this.payment) {
      return;
    }
    
    const chapter: Chapter = this.chapterService.getActiveChapter();

    this.countryService.getCountry(chapter.countryId).subscribe((country: Country) => {
      this.currencySymbol = country.currencySymbol;
      this.changeDetector.detectChanges();
    });
  }

}
