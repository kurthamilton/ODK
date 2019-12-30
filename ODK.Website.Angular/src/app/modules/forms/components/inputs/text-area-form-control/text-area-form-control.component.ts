import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';
import { TextAreaFormControlViewModel } from './text-area-form-control.view-model';

@Component({
  selector: 'app-text-area-form-control',
  templateUrl: './text-area-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextAreaComponent extends InputBase {

  private _rows: number;

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  get rows(): number { return this._rows; }

  protected onInit(): void {
    this._rows = (<TextAreaFormControlViewModel>this.viewModel).rows || 3;
  }
}
