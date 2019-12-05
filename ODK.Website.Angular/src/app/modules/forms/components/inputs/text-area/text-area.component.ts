import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';
import { TextAreaViewModel } from './text-area.view-model';

@Component({
  selector: 'app-text-area',
  templateUrl: './text-area.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextAreaComponent extends InputBase {

  private _rows: number;

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  get rows(): number { return this._rows; }

  protected onInit(): void {
    this._rows = (<TextAreaViewModel>this.viewModel).rows || 3;
  }
}
