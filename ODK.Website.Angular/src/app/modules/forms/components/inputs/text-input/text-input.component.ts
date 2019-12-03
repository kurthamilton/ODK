import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextInputComponent extends InputBase {

  @Input() type: string;

}
