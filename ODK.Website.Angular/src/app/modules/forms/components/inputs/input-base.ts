import { Input, Output, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { DynamicFormControlViewModel } from '../dynamic-form-control.view-model';

export abstract class InputBase {

  private _viewModel: DynamicFormControlViewModel;

  protected constructor(showLabel?: boolean) {
    this.showLabel = showLabel !== false;
  }

  @Input() formGroup: FormGroup;
  @Input() set viewModel(value: DynamicFormControlViewModel) {
    this._viewModel = value;
    this.controlId = value.id;
    if (value.validators) {
      this.pattern = value.validators.pattern;
      this.required = value.validators.required;
    }
  }

  @Output() validate: EventEmitter<void> = new EventEmitter<void>();

  get viewModel(): DynamicFormControlViewModel {
    return this._viewModel;
  }



  controlId: string;  
  match: string;
  pattern: string;
  required: boolean;
  showLabel: boolean;

  onValidate(): void {
    this.validate.emit();
  }
}