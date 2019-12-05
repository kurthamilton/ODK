import { Type } from '@angular/core';

import { FormControlLabelViewModel } from './form-control-label.view-model';
import { FormControlValidatorsViewModel } from './form-control-validators.view-model';
import { InputBase } from './inputs/input-base';

export abstract class DynamicFormControlViewModel {
  private _id: string;
  private _label: FormControlLabelViewModel;

  protected constructor(options: { 
    id: string, 
    label: FormControlLabelViewModel,
    validators?: FormControlValidatorsViewModel
  }) {
    this._id = options.id;
    this._label = options.label;
    this.validators = options.validators;
  }

  get id(): string { return this._id; }
  get label(): FormControlLabelViewModel { return this._label; }
  abstract get type(): Type<InputBase>;
  validators?: FormControlValidatorsViewModel;
  abstract value: any;
}