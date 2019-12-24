import { FormControlLabelViewModel } from './form-control-label/form-control-label.view-model';
import { FormControlValidationViewModel } from './form-control-validation/form-control-validation.view-model';

export interface FormControlOptions {
  id: string;
  label: FormControlLabelViewModel;
  validation?: FormControlValidationViewModel;
}