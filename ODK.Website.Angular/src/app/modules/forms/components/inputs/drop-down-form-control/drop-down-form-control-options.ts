import { DropDownFormControlOption } from './drop-down-form-control-option';
import { FormControlOptions } from '../../form-control-options';

export interface DropDownFormControlOptions extends FormControlOptions {
  options: DropDownFormControlOption[];
  value?: string;
}