import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';
import { FormControlLabelViewModel } from '../../form-control-label.view-model';

@Component({
  selector: 'app-check-box-form-control',
  templateUrl: './check-box-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CheckBoxFormControlComponent extends InputBase {
  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector, false);
  }

  label: FormControlLabelViewModel;

  onInit(): void {
    this.label = {
      class: 'form-check-label',
      helpText: this.viewModel.label.helpText,
      subtitle: this.viewModel.label.subtitle,
      text: this.viewModel.label.text,
      textIsHtml: this.viewModel.label.textIsHtml
    };
  }
}
