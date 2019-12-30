import { FormControlViewModel } from '../../form-control.view-model';
import { ReadOnlyFormControlComponent } from './read-only-form-control.component';
import { ReadOnlyFormControlOptions } from './read-only-form-control-options';

export class ReadOnlyFormControlViewModel extends FormControlViewModel {
  constructor(options: ReadOnlyFormControlOptions) {
    super(options);
    this.controlType = options.controlType;
    this.value = options.value;
  }

  controlType: string;
  get type() { return ReadOnlyFormControlComponent; }
  value: string;
}