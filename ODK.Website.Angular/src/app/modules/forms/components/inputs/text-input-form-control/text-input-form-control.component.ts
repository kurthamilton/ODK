import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { InputBase } from '../input-base';
import { InputGroupPrependViewModel } from '../../input-group-prepend/input-group-prepend.view-model';
import { TextInputFormControlViewModel } from './text-input-form-control.view-model';

@Component({
  selector: 'app-text-input-form-control',
  templateUrl: './text-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextInputFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  @Output() blur: EventEmitter<void> = new EventEmitter<void>();

  get prefix(): InputGroupPrependViewModel {
    return this.viewModel
      ? (this.viewModel as TextInputFormControlViewModel).prefix
      : null;
  }

  get type(): string {
    return this.viewModel
      ? (this.viewModel as TextInputFormControlViewModel).inputType || 'text'
      : 'text';
  }

  onBlur(): void {
    this.blur.emit();
  }
}
