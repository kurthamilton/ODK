import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownMultiFormControlComponent } from './drop-down-multi-form-control.component';
import { DropDownMultiFormControlOptions } from './drop-down-multi-form-control-options';
import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';

export class DropDownMultiFormControlViewModel extends FormControlViewModel {

  constructor(options: DropDownMultiFormControlOptions) {
    super(options);
    this.options = options.options;
    this.value = options.value;
  }

  options: DropDownFormControlOption[];
  get type() { return DropDownMultiFormControlComponent; }
  value: string[];
}