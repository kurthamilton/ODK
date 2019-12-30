import { Type } from '@angular/core';

import { FormControlLabelViewModel } from './form-control-label/form-control-label.view-model';
import { FormControlOptions } from './form-control-options';
import { FormControlValidationViewModel } from './form-control-validation/form-control-validation.view-model';
import { InputBase } from './inputs/input-base';

export abstract class FormControlViewModel {
  private _id: string;
  private _label: FormControlLabelViewModel;

  protected constructor(options: FormControlOptions) {
    this._id = options.id;
    this._label = options.label;
    this.validation = options.validation;
  }

  get id(): string { return this._id; }
  get label(): FormControlLabelViewModel { return this._label; }
  abstract get type(): Type<InputBase>;
  validation?: FormControlValidationViewModel;
  abstract value: any;
}