import { FormControlViewModel } from '../../form-control.view-model';
import { TextInputComponent } from './text-input.component';
import { TextInputFormControlOptions } from './text-input-form-control-options';

export class TextInputViewModel extends FormControlViewModel {
  private _inputType: string;

  constructor(options: TextInputFormControlOptions) {
    super(options);
    this._inputType = options.inputType || 'text';
    this.value = options.value;
  }

  get inputType(): string { return this._inputType; }
  get type() { return TextInputComponent; }
  value: string;
}