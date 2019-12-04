import { Component, OnInit, ChangeDetectionStrategy, Input, ViewChild, ElementRef, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'app-stripe-form',
  templateUrl: './stripe-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StripeFormComponent implements OnInit {

  constructor(private changeDetector: ChangeDetectorRef) { }

  @Input() amount: number;
  @Input() currencyCode: string;
  @Input() currencySymbol: string;
  @Input() publicKey: string;
  @Input() name: string;
  
  @ViewChild('card', { static: true }) card: ElementRef;
  
  error: string;
  token: any;

  private stripe: stripe.Stripe;
  private stripeElement: stripe.elements.Element;

  ngOnInit(): void {
    this.loadRawStripe();
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

  private loadRawStripe(): void {
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
