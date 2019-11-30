import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { LoadingSpinnerOptions } from './loading-spinner-options';

@Component({
  selector: 'app-loading-spinner',
  templateUrl: './loading-spinner.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingSpinnerComponent {
  @Input() options: LoadingSpinnerOptions;

  get overlay(): boolean { return this.options && this.options.overlay; }
  get small(): boolean { return this.options && this.options.small; }
  get spinnerClass(): string { return this.options && this.options.spinnerClass; }
}
