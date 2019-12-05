import { CheckBoxComponent } from './check-box.component';
import { CheckBoxFormControlOptions } from './check-box-form-control-options';
import { FormControlViewModel } from '../../form-control.view-model';

export class CheckBoxViewModel extends FormControlViewModel {
  constructor(options: CheckBoxFormControlOptions) {
    super(options);
    this.value = options.value || false;
  }

  get type() { return CheckBoxComponent; }
  value: boolean;
}