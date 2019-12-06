import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';

@Component({
  selector: 'app-check-box-form-control',
  templateUrl: './check-box-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckBoxFormControlComponent extends InputBase {
  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector, false);
  }
}
