import { FormControlOptions } from '../../form-control-options';

export interface TextInputFormControlOptions extends FormControlOptions {
  inputType?: 'text' | 'date' | 'password';
  value?: string;
}