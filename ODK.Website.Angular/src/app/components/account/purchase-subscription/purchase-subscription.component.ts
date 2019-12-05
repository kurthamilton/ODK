import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter } from '@angular/core';
import { SafeHtml, DomSanitizer } from '@angular/platform-browser';

import { Observable } from 'rxjs';

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
  @Input() close: Observable<void>;
  @Output() purchase: EventEmitter<string> = new EventEmitter<string>();

  description: SafeHtml;
  payment: Payment;

  ngOnChanges(): void {
    if (!this.chapterSubscription) {
      return;
    }

    this.description = this.sanitizer.bypassSecurityTrustHtml(this.chapterSubscription.description);
    this.payment = {
      amount: this.chapterSubscription.amount,
      name: this.chapterSubscription.title
    };
  }

  onCardSubmit(token: string): void {
    this.purchase.emit(token);
  }
}
