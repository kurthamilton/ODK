import { FormControlType } from './form-control-type';

export interface FormControlViewModel {
  dropDown?: {
    default?: string;
    freeTextOption?: string;
    options: string[];
  };
  helpText?: string;
  id: string;
  label: string;
  subtitle?: string;
  type?: FormControlType;
  value?: string;
  validators?: {
    pattern?: string;
    required?: boolean;
  }
}
