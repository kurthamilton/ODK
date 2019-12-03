import { Type } from '@angular/core';

import { InputBase } from './inputs/input-base';
import { DynamicFormControlLabelViewModel } from './dynamic-form-control-label.view-model';

export interface DynamicFormControlViewModel {
  componentFactory: Type<InputBase>;
  controlId: string;
  label: DynamicFormControlLabelViewModel;
  validators?: {
    pattern?: string;
    required?: boolean;
  }
  value: string;
}