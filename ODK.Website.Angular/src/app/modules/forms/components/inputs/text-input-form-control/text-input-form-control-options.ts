import { FormControlOptions } from '../../form-control-options';

export interface TextInputFormControlOptions extends FormControlOptions {
  inputType?: 'text' | 'date' | 'password' | 'number';
  prefix?: {
    icon: string;
    text: string;
  };
  value?: string;
}