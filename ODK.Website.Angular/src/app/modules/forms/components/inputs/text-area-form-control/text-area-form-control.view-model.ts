import { FormControlViewModel } from '../../form-control.view-model';
import { TextAreaComponent } from './text-area-form-control.component';
import { TextAreaFormControlOptions } from './text-area-form-control-options';

export class TextAreaFormControlViewModel extends FormControlViewModel {
  constructor(options: TextAreaFormControlOptions) {
    super(options);

    this.rows = options.rows || 3;
    this.value = options.value;
  }

  rows: number;
  get type() { return TextAreaComponent; }
  value: string;
}