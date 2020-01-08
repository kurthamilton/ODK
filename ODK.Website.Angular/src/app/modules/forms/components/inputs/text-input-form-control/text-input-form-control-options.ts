import { FormControlOptions } from '../../form-control-options';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';
import { TextInputType } from './text-input-type';

export interface TextInputFormControlOptions extends FormControlOptions {
  inputType?: TextInputType;
  prefix?: InputGroupPrependViewModel;
  value?: string;
}