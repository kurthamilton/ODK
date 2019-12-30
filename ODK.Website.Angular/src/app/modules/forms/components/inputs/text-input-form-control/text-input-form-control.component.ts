import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { InputBase } from '../input-base';
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

  get prefix(): { icon: string; text: string; } {
    return this.viewModel
      ? (<TextInputFormControlViewModel>this.viewModel).prefix
      : null;
  }

  get type(): string {
    return this.viewModel
      ? (<TextInputFormControlViewModel>this.viewModel).inputType || 'text'
      : 'text';
  }

  onBlur(): void {
    this.blur.emit();
  }
}
