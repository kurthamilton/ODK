import { DynamicFormControlViewModel } from '../../dynamic-form-control.view-model';
import { TextInputComponent } from './text-input.component';
import { FormControlValidatorsViewModel } from '../../form-control-validators.view-model';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';

export class TextInputViewModel extends DynamicFormControlViewModel {
  private _inputType: string;

  constructor(options: {
    id: string,
    inputType?: 'text' | 'date' | 'password',
    label: FormControlLabelViewModel,
    validators?: FormControlValidatorsViewModel,
    value?: string
  }) {
    super(options);
    this._inputType = options.inputType || 'text';
    this.value = options.value;
  }

  get inputType(): string { return this._inputType; }
  get type() { return TextInputComponent; }
  value: string;
}