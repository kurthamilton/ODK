import { CheckBoxComponent } from './check-box.component';
import { DynamicFormControlViewModel } from '../../dynamic-form-control.view-model';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';

export class CheckBoxViewModel extends DynamicFormControlViewModel {
  constructor(options: { id: string, label: FormControlLabelViewModel, value?: boolean }) {
    super(options);
    this.value = options.value || false;
  }

  get type() { return CheckBoxComponent; }
  value: boolean;
}