import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { FormControlOptions } from 'src/app/modules/forms/components/form-control-options';

export interface DropDownMultiFormControlOptions extends FormControlOptions {
  options: DropDownFormControlOption[];
  value?: string[];
}
