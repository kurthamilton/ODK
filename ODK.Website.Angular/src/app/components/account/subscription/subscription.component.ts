import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  selector: 'app-subscription',
  templateUrl: './subscription.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubscriptionComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
