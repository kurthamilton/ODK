import { Component, ChangeDetectionStrategy, Input, OnChanges, ChangeDetectorRef, OnDestroy, Output, EventEmitter } from '@angular/core';
import { FormGroup, AbstractControl, FormControl, ValidatorFn, Validators, FormBuilder } from '@angular/forms';

import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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
  match: string;
  pattern: string;
  required: boolean;
  showLabel: boolean;
  type: FormControlType;

  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }
    
    this.match = '';
    this.pattern = this.viewModel.validators ? this.viewModel.validators.pattern : '';
    this.required = !!this.viewModel.validators && this.viewModel.validators.required === true;
    this.type = this.viewModel.type || 'text';
    
    this.showLabel = this.type !== 'checkbox';

    this.control = this.createFormControl();
    this.formGroup.addControl(this.viewModel.id, this.control);
  }

  ngOnDestroy(): void {
    this.destroyed.next({});
  }

  validate(): void {
    this.control.updateValueAndValidity();
  }

  private createFormControl(): FormControl {
    const validators: ValidatorFn[] = [];

    if (this.required) {
      validators.push(Validators.required);
    }
    
    if (this.pattern) {
      validators.push(Validators.pattern(this.pattern));
    }
    
    const control: FormControl = this.formBuilder.control(this.viewModel.value, validators);

    // bind value updates back to view models
    control.valueChanges
      .pipe(
        takeUntil(this.updated),
        takeUntil(this.destroyed)
      )
      .subscribe((value: any) => this.onValueChanged(value));

    return control;
  }

  private onValueChanged(value: string): void {    
    this.viewModel.value = value;
    this.changeDetector.detectChanges();
  }
}
