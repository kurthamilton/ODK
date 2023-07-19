import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

import { AccountService } from 'src/app/services/account/account.service';
import { LoadingSpinnerOptions } from '../../elements/loading-spinner/loading-spinner-options';
import { Script } from 'src/app/core/scripts/script';
import { ScriptService } from 'src/app/services/scripts/script.service';

declare var paypal: any;

@Component({
  selector: 'app-paypal-form',
  templateUrl: './paypal-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaypalFormComponent implements OnInit {

  constructor(
    private changeDetector: ChangeDetectorRef,
    private scriptService: ScriptService,
    private accountService: AccountService
  ) {
  }

  @Input() amount: number;
  @Input() currencyCode: string;
  @Input() currencySymbol: string;
  @Input() publicKey: string;
  @Input() name: string;
  @Input() title: string;
  @Output() cardSubmit: EventEmitter<string> = new EventEmitter<string>();
  @Output() close: EventEmitter<boolean> = new EventEmitter<boolean>();

  error: string;
  loadingOptions: LoadingSpinnerOptions = {
    overlay: true
  };
  paying = false;
  token: any;
  showSuccess = false;

  ngOnInit(): void {
    const clientId: string = this.publicKey;
    const script: Script = {
      name: `paypal-${clientId}`,
      src: `https://www.paypal.com/sdk/js?client-id=${clientId}&currency=${this.currencyCode}`
    };

    this.scriptService.load(script)
      .then(() => {
        this.loadForm();
        this.changeDetector.detectChanges();
      })
      .catch((e) => {
        console.log(e);
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
  }

  private loadForm(): void {
    paypal.Buttons({
      style: {
        layout: 'vertical',
        color:  'gold',
        shape:  'rect',
        label:  'paypal'
      },
      createOrder: (data, actions) => {
        // Set up the transaction
        return actions.order.create({
          purchase_units: [{
            amount: {
              currency_code: this.currencyCode,
              value: this.amount.toString()
            }
          }]
        });
      },
      onApprove: (data, actions) => {
        const orderId = data.orderID;
        this.cardSubmit.emit(orderId);
      }
    }).render('#paypal-button-container');
  }

}
