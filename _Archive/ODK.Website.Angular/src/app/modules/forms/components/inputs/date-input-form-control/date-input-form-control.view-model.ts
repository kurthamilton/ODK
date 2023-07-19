import { Type } from '@angular/core';

import { DateInputFormControlComponent } from './date-input-form-control.component';
import { DateInputFormControlOptions } from './date-input-form-control-options';
import { FormControlViewModel } from '../../form-control.view-model';
import { InputBase } from '../input-base';

export class DateInputFormControlViewModel extends FormControlViewModel {
  constructor(options: DateInputFormControlOptions) {
    super(options);
    this.value = options.value;
  }

  get type(): Type<InputBase> { return DateInputFormControlComponent; }
  value: Date;
}
