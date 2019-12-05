import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';

@Component({
  selector: 'app-check-box',
  templateUrl: './check-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckBoxComponent extends InputBase {
  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector, false);
  }
}
