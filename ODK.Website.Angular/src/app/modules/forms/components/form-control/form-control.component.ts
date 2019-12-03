import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef, OnDestroy, Output, EventEmitter } from '@angular/core';
import { FormGroup, AbstractControl, ValidatorFn, Validators, FormBuilder } from '@angular/forms';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormControlViewModel } from '../form-control.view-model';
import { FormControlType } from '../form-control-type';

@Component({
  selector: 'app-form-control',
  templateUrl: './form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormControlComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private formBuilder: FormBuilder
  ) {
  }

  @Input() formGroup: FormGroup;
  @Input() updated: Observable<boolean>;
  @Input() validated: boolean;
  @Input() viewModel: FormControlViewModel;
  @Output() valueChange: EventEmitter<string> = new EventEmitter<string>();

  control: AbstractControl;
  childControls: FormControlViewModel[] = [];
  match: string;
  pattern: string;
  required: boolean;
  selectedOption: string;
  showLabel: boolean;
  type: FormControlType;

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }

    this.match = '';
    this.pattern = this.viewModel.validators ? this.viewModel.validators.pattern : '';
    this.required = !!this.viewModel.validators && this.viewModel.validators.required === true;
    this.type = this.viewModel.type || 'text';

    this.showLabel = this.type !== 'checkbox';

    this.buildDropDown(this.viewModel.value);

    const value: string = this.getValue();

    this.control = this.createFormControl(value);

    if (!this.formGroup.get(this.viewModel.id)) {
      this.formGroup.addControl(this.viewModel.id, this.control);
    }
  }

  ngOnDestroy(): void {}

  onChildControlValueChanged(viewModel: FormControlViewModel): void {
    if (this.type === 'dropdown') {
      this.bindValue(viewModel.value);
    }
  }

  validate(): void {
    this.control.updateValueAndValidity();
  }

  private addDropDownFreeTextControl(value: string): void {
    const viewModel: FormControlViewModel = {
      id: `${this.viewModel.id}-freetext`,
      label: 'Please specify',
      validators: this.viewModel.validators,
      value: value
    };

    this.childControls.push(viewModel);
  }

  private bindValue(value: string): void {
    if (this.type === 'dropdown' && value === this.viewModel.dropDown.freeTextOption) {
      value = this.childControls[0].value;
    }
    this.viewModel.value = value;

    this.valueChange.emit(this.viewModel.value);
    this.changeDetector.detectChanges();
  }

  private buildDropDown(value: string): void {
    if (this.viewModel.type !== 'dropdown') {
      return;
    }

    this.childControls.length = 0;

    this.selectedOption = this.viewModel.dropDown.options.find(x => x === value);
    if (this.selectedOption) {
      if (value === this.viewModel.dropDown.freeTextOption) {
        this.addDropDownFreeTextControl('');
      }
      return;
    }

    if (!value || !this.viewModel.dropDown.freeTextOption) {
      this.selectedOption = this.viewModel.dropDown.default;
      return;
    }

    this.selectedOption = this.viewModel.dropDown.freeTextOption;
    this.addDropDownFreeTextControl(value);
  }

  private createFormControl(value: string): AbstractControl {
    const validators: ValidatorFn[] = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.pattern) {
      validators.push(Validators.pattern(this.pattern));
    }


    let control: AbstractControl = this.formGroup.get(this.viewModel.id);
    if (!control) {
      control = this.formBuilder.control(value, validators);
    }

    // bind value updates back to view models
    control.valueChanges
      .pipe(
        takeUntil(this.updated),
        takeUntil(componentDestroyed(this))
      )
      .subscribe(x => this.onValueChanged(x));

    return control;
  }

  private getValue(): string {
    if (this.type === 'dropdown') {
      return this.selectedOption;
    }

    return this.viewModel.value;
  }

  private onValueChanged(value: string): void {
    this.buildDropDown(value);
    this.bindValue(value);
  }
}
