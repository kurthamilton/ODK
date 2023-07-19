import { Component, OnInit, ChangeDetectionStrategy, Input, ViewChild, ElementRef, ChangeDetectorRef, Output, EventEmitter, OnDestroy } from '@angular/core';

import { LoadingSpinnerOptions } from 'src/app/modules/shared/components/elements/loading-spinner/loading-spinner-options';
import { ScriptService, appScripts } from 'src/app/services/scripts/script.service';

@Component({
  selector: 'app-stripe-form',
  templateUrl: './stripe-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StripeFormComponent implements OnInit, OnDestroy {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private scriptService: ScriptService
  ) {
  }

  @Input() amount: number;
  @Input() currencyCode: string;
  @Input() currencySymbol: string;
  @Input() publicKey: string;
  @Input() name: string;
  @Output() cardSubmit: EventEmitter<string> = new EventEmitter<string>();
  @Output() close: EventEmitter<boolean> = new EventEmitter<boolean>();

  @ViewChild('card') card: ElementRef;

  error: string;
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true
  };
  paying = false;
  token: any;

  private stripe: stripe.Stripe;
  private stripeElement: stripe.elements.Element;

  ngOnInit(): void {
    this.scriptService.load(appScripts.stripe)
      .then(() => {
        this.loadStripeForm();
        this.changeDetector.detectChanges();
      })
      .catch((e) => {
        this.error = 'An error has occurred while loading the form';
        this.changeDetector.detectChanges();
      });
  }

  ngOnDestroy(): void {}

  onClose(): void {
    this.close.emit(true);
  }

  onSubmit(): void {
    this.paying = true;
    this.changeDetector.detectChanges();

    this.stripe.createToken(this.stripeElement).then((result: stripe.TokenResponse) => {
      if (result.error) {
        this.paying = false;
        this.error = result.error.message;
        this.changeDetector.detectChanges();
      } else {
        this.cardSubmit.emit(result.token.id);
      }
    });
  }

  private loadStripeForm(): void {
    this.stripe = Stripe(this.publicKey);

    const elements: stripe.elements.Elements = this.stripe.elements();

    this.stripeElement = elements.create('card', {
      hidePostalCode: true
    });

    this.stripeElement.mount(this.card.nativeElement);
    this.stripeElement.on('change', (event: stripe.elements.ElementChangeResponse) => {
      this.error = event.error ? event.error.message : '';
      this.changeDetector.detectChanges();
    });
  }
}
