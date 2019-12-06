import { Type } from '@angular/core';

import { GoogleMapsTextInputComponent } from './google-maps-text-input.component';
import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';
import { TextInputViewModel } from 'src/app/modules/forms/components/inputs/text-input/text-input.view-model';

export class GoogleMapsTextInputViewModel extends TextInputViewModel {
  get type(): Type<InputBase> { return GoogleMapsTextInputComponent; }
}