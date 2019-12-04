import { Component, OnInit, ChangeDetectionStrategy, Input, ViewChild, ElementRef, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { ScriptService, appScripts } from 'src/app/services/scripts/script.service';

@Component({
  selector: 'app-stripe-form',
  templateUrl: './stripe-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StripeFormComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef,
    private scriptService: ScriptService
  ) {
  }

  @Input() amount: number;
  @Input() currencyCode: string;
  @Input() currencySymbol: string;
  @Input() publicKey: string;
  @Input() name: string;
  @Output() close: EventEmitter<boolean> = new EventEmitter<boolean>();

  @ViewChild('card', { static: true }) card: ElementRef;

  error: string;
  token: any;

  private stripe: stripe.Stripe;
  private stripeElement: stripe.elements.Element;

  ngOnInit(): void {
    this.scriptService.load(appScripts.stripe)
      .then(() => {
        this.loadStripeForm();
        this.changeDetector.detectChanges();
      })
      .catch(() => {
        this.error = 'An error has occurred while loading the form'
        this.changeDetector.detectChanges();
      });
  }

  onClose(): void {
    this.close.emit(true);
  }

  onSubmit(): void {
    this.stripe.createToken(this.stripeElement).then((result: stripe.TokenResponse) => {
      if (result.error) {
        this.error = result.error.message;
        this.changeDetector.detectChanges();
      } else {
        console.log('Token acquired!');
        console.log(result.token);
        console.log(result.token.id);
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
