import { CheckBoxFormControlComponent } from './check-box-form-control.component';
import { CheckBoxFormControlOptions } from './check-box-form-control-options';
import { FormControlViewModel } from '../../form-control.view-model';

export class CheckBoxFormControlViewModel extends FormControlViewModel {
  constructor(options: CheckBoxFormControlOptions) {
    super(options);
    this.value = options.value || false;
  }

  get type() { return CheckBoxFormControlComponent; }
  value: boolean;
}
