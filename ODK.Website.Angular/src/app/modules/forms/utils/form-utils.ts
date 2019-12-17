import { AbstractControl, FormBuilder, FormGroup, ValidatorFn, Validators } from '@angular/forms';

import { FormControlViewModel } from '../components/form-control.view-model';

export class FormUtils {
  private static formBuilder: FormBuilder = new FormBuilder();

  static createControl(formGroup: FormGroup, viewModel: FormControlViewModel): AbstractControl {
    if (!formGroup) {
      return;
    }
    
    let control: AbstractControl = formGroup.get(viewModel.id);
    if (control) {
      return control;
    }

    const validators: ValidatorFn[] = [];

    if (viewModel.validation) {
      if (viewModel.validation.required) {
        validators.push(Validators.required);
      }

      if (viewModel.validation.pattern) {
        validators.push(Validators.pattern(viewModel.validation.pattern));
      }
    }

    control = this.formBuilder.control(viewModel.value, validators);
    formGroup.addControl(viewModel.id, control);

    return control;
  }
}