import { FormControlOptions } from '../../form-control-options';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';

export interface NumberInputFormControlOptions extends FormControlOptions {
  min?: number;
  prefix?: InputGroupPrependViewModel;
  step?: number;
  value?: number;
}