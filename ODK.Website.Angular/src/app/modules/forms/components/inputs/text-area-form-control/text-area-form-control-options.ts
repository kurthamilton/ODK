import { FormControlOptions } from '../../form-control-options';

export interface TextAreaFormControlOptions extends FormControlOptions {
  rows?: number;
  value?: string;
}