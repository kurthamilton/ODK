import { Type } from '@angular/core';

import { FormControlViewModel } from '../../form-control.view-model';
import { InputBase } from '../input-base';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';
import { TextInputFormControlComponent } from './text-input-form-control.component';
import { TextInputFormControlOptions } from './text-input-form-control-options';

export class TextInputFormControlViewModel extends FormControlViewModel {
  private _inputType: string;

  constructor(options: TextInputFormControlOptions) {
    super(options);
    this._inputType = options.inputType || 'text';
    this.prefix = options.prefix;
    this.value = options.value;
  }

  get inputType(): string { return this._inputType; }
  prefix: InputGroupPrependViewModel;
  get type(): Type<InputBase> { return TextInputFormControlComponent; }
  value: string;
}