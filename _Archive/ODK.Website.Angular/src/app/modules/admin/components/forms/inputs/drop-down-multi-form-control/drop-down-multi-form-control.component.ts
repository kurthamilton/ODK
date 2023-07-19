import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownMultiFormControlViewModel } from './drop-down-multi-form-control.view-model';
import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';

@Component({
  selector: 'app-drop-down-multi-form-control',
  templateUrl: './drop-down-multi-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropDownMultiFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  options: DropDownFormControlOption[];
  selectedOptions: string[];

  private dropDownMultiFormControlViewModel: DropDownMultiFormControlViewModel;

  protected onInit(): void {
    this.dropDownMultiFormControlViewModel = this.viewModel as DropDownMultiFormControlViewModel;

    this.options = this.dropDownMultiFormControlViewModel.options;
    this.selectedOptions = this.viewModel.value;
  }
}
