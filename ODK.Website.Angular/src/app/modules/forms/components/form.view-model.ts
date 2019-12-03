import { FormControlViewModel } from './form-control.view-model';

import { Observable } from 'rxjs';

export interface FormViewModel {
  buttonText: string;
  callback: Observable<boolean | string[]>;
  formControls: FormControlViewModel[];
  showUpdating?: boolean;
  valid?: boolean;
  validated?: boolean;
}
