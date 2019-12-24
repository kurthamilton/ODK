import { Observable } from 'rxjs';

import { FormControlViewModel } from '../form-control.view-model';
import { FormButtonViewModel } from '../form-button/form-button.view-model';

export interface FormViewModel {
  buttons: FormButtonViewModel[];
  callback: Observable<boolean | string[]>;
  controls: FormControlViewModel[];
}