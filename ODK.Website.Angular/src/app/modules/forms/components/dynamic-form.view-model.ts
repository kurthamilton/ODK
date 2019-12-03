import { Observable } from 'rxjs';

import { DynamicFormControlViewModel } from './dynamic-form-control.view-model';

export interface DynamicFormViewModel {
  buttonText: string;
  callback: Observable<boolean | string[]>;
  controls: DynamicFormControlViewModel[];
  showUpdating?: boolean;
  valid?: boolean;
  validated?: boolean;
}