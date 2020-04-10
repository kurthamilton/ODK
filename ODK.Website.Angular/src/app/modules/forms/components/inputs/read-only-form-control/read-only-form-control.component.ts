import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';
import { ReadOnlyFormControlViewModel } from './read-only-form-control.view-model';

@Component({
  selector: 'app-read-only-form-control',
  templateUrl: './read-only-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ReadOnlyFormControlComponent extends InputBase {
  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  get controlType(): string { return (this.viewModel as ReadOnlyFormControlViewModel).controlType; }
}
