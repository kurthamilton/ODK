import { DropDownFormControlComponent } from './drop-down-form-control.component';
import { DynamicFormControlViewModel } from '../../dynamic-form-control.view-model';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';
import { FormControlValidatorsViewModel } from '../../form-control-validators.view-model';

export class DropDownFormControlViewModel extends DynamicFormControlViewModel {
  
  constructor(options: { 
    id: string, 
    label: FormControlLabelViewModel,
    validators: FormControlValidatorsViewModel,
    options: {
      default: string,
      freeTextOption: string,
      options: string[]
    }
  }) {
    super(options);
  }

  get type() { return DropDownFormControlComponent; }
  value: string;
}