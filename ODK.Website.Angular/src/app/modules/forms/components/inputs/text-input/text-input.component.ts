import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';

import { InputBase } from '../input-base';
import { TextInputViewModel } from './text-input.view-model';

@Component({
  selector: 'app-text-input',
  templateUrl: './text-input.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextInputComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  @Output() blur: EventEmitter<void> = new EventEmitter<void>();

  get type(): string {
    return this.viewModel
      ? (<TextInputViewModel>this.viewModel).inputType || 'text'
      : 'text';
  }

  onBlur(): void {
    this.blur.emit();
  }
}
