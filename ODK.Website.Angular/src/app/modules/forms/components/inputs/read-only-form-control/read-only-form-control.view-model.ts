import { DynamicFormControlViewModel } from '../../dynamic-form-control.view-model';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';
import { ReadOnlyFormControlComponent } from './read-only-form-control.component';

export class ReadOnlyFormControlViewModel extends DynamicFormControlViewModel {
  constructor(options: { id: string, label: FormControlLabelViewModel, value: string }) {
    super(options);
    this.value = options.value;
  }

  get type() { return ReadOnlyFormControlComponent; }
  value: string;
}