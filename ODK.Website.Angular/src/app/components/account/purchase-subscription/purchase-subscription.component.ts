import { Component, ChangeDetectionStrategy, Input, OnChanges } from '@angular/core';
import { SafeHtml, DomSanitizer } from '@angular/platform-browser';

import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Payment } from 'src/app/core/payments/payment';

@Component({
  selector: 'app-purchase-subscription',
  templateUrl: './purchase-subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseSubscriptionComponent implements OnChanges {

  constructor(private sanitizer: DomSanitizer) {     
  }

  @Input() chapterSubscription: ChapterSubscription;

  description: SafeHtml;
  payment: Payment;

  ngOnChanges(): void {
    if (!this.chapterSubscription) {
      return;
    }
        
    this.description = this.sanitizer.bypassSecurityTrustHtml(this.chapterSubscription.description);
    this.payment = {
      amount: this.chapterSubscription.amount
    };
  }
}
