import { Type } from '@angular/core';

import { InputBase } from './inputs/input-base';
import { FormControlLabelViewModel } from './form-control-label.view-model';
import { FormControlValidatorsViewModel } from './form-control-validators.view-model';

export interface DynamicFormControlViewModel {
  id: string;
  label: FormControlLabelViewModel;
  type: Type<InputBase>;
  validators?: FormControlValidatorsViewModel;
  value?: string;
}