import { Type } from '@angular/core';

import { FormControlViewModel } from '../../form-control.view-model';
import { InputBase } from '../input-base';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';
import { NumberInputFormControlComponent } from './number-input-form-control.component';
import { NumberInputFormControlOptions } from './number-input-form-control-options';

export class NumberInputFormControlViewModel extends FormControlViewModel {

  constructor(options: NumberInputFormControlOptions) {
    super(options);
    this.min = options.min;
    this.prefix = options.prefix;
    this.step = options.step;
    this.value = options.value;
  }

  min: number;
  prefix: InputGroupPrependViewModel;
  step: number;
  get type(): Type<InputBase> { return NumberInputFormControlComponent; }
  value: number;
}
