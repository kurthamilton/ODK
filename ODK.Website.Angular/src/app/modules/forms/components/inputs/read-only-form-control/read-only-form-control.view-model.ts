import { FormControlViewModel } from '../../form-control.view-model';
import { ReadOnlyFormControlComponent } from './read-only-form-control.component';
import { ReadOnlyFormControlOptions } from './read-only-form-control-options';

export class ReadOnlyFormControlViewModel extends FormControlViewModel {
  constructor(options: ReadOnlyFormControlOptions) {
    super(options);
    this.value = options.value;
  }

  get type() { return ReadOnlyFormControlComponent; }
  value: string;
}