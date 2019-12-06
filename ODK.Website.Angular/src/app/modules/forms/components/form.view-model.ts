import { Observable } from 'rxjs';

import { FormControlViewModel } from './form-control.view-model';

export interface FormViewModel {
  buttonText: string;
  callback: Observable<boolean | string[]>;
  controls: FormControlViewModel[];
}