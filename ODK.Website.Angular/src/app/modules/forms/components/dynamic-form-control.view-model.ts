import { Type } from '@angular/core';

import { InputBase } from './inputs/input-base';
import { FormControlLabelViewModel } from './form-control-label.view-model';
import { FormControlValidatorsViewModel } from './form-control-validators.view-model';

export interface DynamicFormControlViewModel {
  componentFactory: Type<InputBase>;
  id: string;
  label: FormControlLabelViewModel;
  validators?: FormControlValidatorsViewModel;
  value: string;
}