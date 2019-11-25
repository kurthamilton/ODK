import { FormControlType } from './form-control-type';

export interface FormControlViewModel {
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
  