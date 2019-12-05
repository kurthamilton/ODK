import { Component, ChangeDetectionStrategy } from '@angular/core';

import { InputBase } from '../input-base';

@Component({
  selector: 'app-check-box',
  templateUrl: './check-box.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckBoxComponent extends InputBase {
  constructor() {
    super(false);
  }
}
