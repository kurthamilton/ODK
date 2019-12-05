import { DropDownFormControlComponent } from './drop-down-form-control.component';
import { DropDownFormControlOption } from './drop-down-form-control-option';
import { DropDownFormControlOptions } from './drop-down-form-control-options';
import { FormControlViewModel } from '../../form-control.view-model';

export class DropDownFormControlViewModel extends FormControlViewModel {

  constructor(options: DropDownFormControlOptions) {
    super(options);
    this.options = options.options;
    this.value = options.value;
  }

  options: DropDownFormControlOption[];
  get type() { return DropDownFormControlComponent; }
  value: string;

  setValue(option: DropDownFormControlOption, freeText: string): void {
    if (!option) {
      return;
    }

    this.value = option.freeText ? freeText : option.value;
  }
}