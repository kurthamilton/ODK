import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { InputBase } from '../input-base';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';
import { NumberInputFormControlViewModel } from './number-input-form-control.view-model';

@Component({
  selector: 'app-number-input-form-control',
  templateUrl: './number-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NumberInputFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  @Output() blur: EventEmitter<void> = new EventEmitter<void>();

  get min(): number {
    return this.viewModel
      ? this.numberInputViewModel.min
      : null;
  }

  get prefix(): InputGroupPrependViewModel {
    return this.viewModel
      ? this.numberInputViewModel.prefix
      : null;
  }

  get step(): number {
    return this.viewModel
      ? this.numberInputViewModel.step
      : null;
  }

  private get numberInputViewModel(): NumberInputFormControlViewModel {
    return this.viewModel as NumberInputFormControlViewModel;
  }

  onBlur(): void {
    this.blur.emit();
  }
}
