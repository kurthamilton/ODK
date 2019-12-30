import { FormControlOptions } from '../../form-control-options';

export interface ReadOnlyFormControlOptions extends FormControlOptions {
  controlType?: '' | 'url';
  value?: string;
}