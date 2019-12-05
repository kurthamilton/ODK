import { FormControlLabelViewModel } from './form-control-label.view-model';
import { FormControlType } from './form-control-type';
import { FormControlValidatorsViewModel } from './form-control-validators.view-model';

export interface FormControlViewModel {
  dropDown?: {
    default?: string;
    freeTextOption?: string;
    options: string[];
  };
  id: string;
  label: FormControlLabelViewModel;
  type?: FormControlType;
  validators?: FormControlValidatorsViewModel;
  value?: string;
}
