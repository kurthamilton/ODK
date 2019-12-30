import { FormControlLabelType } from './form-control-label-type';

export interface FormControlLabelViewModel {
  class?: string;
  helpText?: string;    
  subtitle?: string;
  text: string;
  textIsHtml?: boolean;
  type?: FormControlLabelType;
}