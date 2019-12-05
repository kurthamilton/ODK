import { DynamicFormControlViewModel } from '../../dynamic-form-control.view-model';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';
import { FormControlValidatorsViewModel } from '../../form-control-validators.view-model';
import { TextAreaComponent } from './text-area.component';

export class TextAreaViewModel extends DynamicFormControlViewModel {
  constructor(options: { 
    id: string, 
    label: FormControlLabelViewModel, 
    rows?: number,
    validators?: FormControlValidatorsViewModel,
    value?: string
  }) {
    super(options);
    this.rows = options.rows || 3;
    this.value = options.value;
  }

  rows: number;
  get type() { return TextAreaComponent; }
  value: string;
}