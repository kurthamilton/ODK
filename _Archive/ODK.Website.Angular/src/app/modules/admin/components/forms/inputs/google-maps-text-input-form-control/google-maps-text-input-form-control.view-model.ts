import { Type } from '@angular/core';

import { GoogleMapsTextInputFormControlComponent } from './google-maps-text-input-form-control.component';
import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

export class GoogleMapsTextInputFormControlViewModel extends TextInputFormControlViewModel {
  get type(): Type<InputBase> { return GoogleMapsTextInputFormControlComponent; }
}
