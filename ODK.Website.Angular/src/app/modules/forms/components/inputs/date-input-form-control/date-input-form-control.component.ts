import { DatePipe } from '@angular/common';
import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { InputBase } from '../input-base';
import { DateInputFormControlViewModel } from './date-input-form-control.view-model';

@Component({
  selector: 'app-date-input-form-control',
  templateUrl: './date-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateInputFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef,
    private datePipe: DatePipe) { 
    super(changeDetector);
  }

  private dateInputFormControlViewModel: DateInputFormControlViewModel;

  protected onInit(): void {
    this.dateInputFormControlViewModel = <DateInputFormControlViewModel>this.viewModel;
    this.control.setValue(this.datePipe.transform(this.viewModel.value, 'yyyy-MM-dd'));
  }

  protected setValue(value: string): void {
    this.dateInputFormControlViewModel.value = value ? new Date(value) : null;
  }
}
