import { FormControlLabelViewModel } from './form-control-label.view-model';
import { FormControlValidationViewModel } from './form-control-validation.view-model';

export interface FormControlOptions {
  id: string;
  label: FormControlLabelViewModel;
  validation?: FormControlValidationViewModel;
}