import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter } from '@angular/core';

import { Observable } from 'rxjs';

import { ChapterSubscription } from 'src/app/core/chapters/chapter-subscription';
import { Payment } from 'src/app/core/payments/payment';

@Component({
  selector: 'app-purchase-subscription',
  templateUrl: './purchase-subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PurchaseSubscriptionComponent implements OnChanges {

  @Input() chapterSubscription: ChapterSubscription;
  @Input() close: Observable<void>;
  @Input() title: string;
  @Output() purchase: EventEmitter<string> = new EventEmitter<string>();

  payment: Payment;

  ngOnChanges(): void {
    if (!this.chapterSubscription) {
      return;
    }

    this.payment = {
      amount: this.chapterSubscription.amount,
      name: this.chapterSubscription.title
    };
  }

  onCardSubmit(token: string): void {
    this.purchase.emit(token);
  }
}
