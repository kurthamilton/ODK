import { FormControlType } from './form-control-type';
import { FormControlLabelViewModel } from './form-control-label.view-model';
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
  value?: string;
  validators?: FormControlValidatorsViewModel;
}
