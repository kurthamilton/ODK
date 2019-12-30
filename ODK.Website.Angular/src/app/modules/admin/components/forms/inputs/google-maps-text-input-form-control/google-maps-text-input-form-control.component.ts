import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';

@Component({
  selector: 'app-google-maps-text-input-form-control',
  templateUrl: './google-maps-text-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class GoogleMapsTextInputFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) { 
    super(changeDetector);
  }

  query: string;

  onBlur(): void {
    this.query = this.viewModel.value;
    this.changeDetector.detectChanges();
  }

  onInit(): void {
    this.query = this.viewModel.value;
    this.changeDetector.detectChanges();
    super.onInit();
  }
}
