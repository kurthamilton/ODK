import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { NgbDateStruct } from '@ng-bootstrap/ng-bootstrap';

import { DateInputFormControlViewModel } from './date-input-form-control.view-model';
import { InputBase } from '../input-base';

@Component({
  selector: 'app-date-input-form-control',
  templateUrl: './date-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DateInputFormControlComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  private dateInputFormControlViewModel: DateInputFormControlViewModel;

  protected onInit(): void {
    this.dateInputFormControlViewModel = this.viewModel as DateInputFormControlViewModel;

    const date: Date = this.viewModel.value ? new Date(this.viewModel.value) : null;
    const ngbDate: NgbDateStruct = date
      ? { year: date.getFullYear(), month: date.getMonth() + 1, day: date.getDate() }
      : null;
    this.control.setValue(ngbDate);
  }

  protected setValue(value: NgbDateStruct): void {
    const date: Date = value
      ? new Date(value.year, value.month - 1, value.day)
      : null;
    this.dateInputFormControlViewModel.value = value ? date : null;
  }
}
