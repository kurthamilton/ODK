import { Component, ChangeDetectionStrategy } from '@angular/core';

import { InputBase } from '../input-base';
import { TextAreaViewModel } from './text-area.view-model';

@Component({
  selector: 'app-text-area',
  templateUrl: './text-area.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TextAreaComponent extends InputBase {
  constructor() {
    super();
  }

  get rows(): number {
    return this.viewModel 
      ? (<TextAreaViewModel>this.viewModel).rows || 3 
      : 3;
  }
}
