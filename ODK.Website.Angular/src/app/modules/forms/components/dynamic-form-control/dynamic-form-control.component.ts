import { Component, ChangeDetectionStrategy, Input, OnChanges, OnDestroy, ChangeDetectorRef, Output, EventEmitter, ViewChild, ViewContainerRef, ComponentFactory, ComponentFactoryResolver, ComponentRef } from '@angular/core';
import { FormBuilder, FormGroup, AbstractControl, ValidatorFn, Validators } from '@angular/forms';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { DynamicFormControlViewModel } from '../dynamic-form-control.view-model';
import { InputBase } from '../inputs/input-base';

@Component({
  selector: 'app-dynamic-form-control',
  templateUrl: './dynamic-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicFormControlComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef,
    private formBuilder: FormBuilder,
    private resolver: ComponentFactoryResolver
  ) {
  }

  @Input() formGroup: FormGroup;
  @Input() updated: Observable<boolean>;
  @Input() validated: boolean;
  @Input() viewModel: DynamicFormControlViewModel;
  @Output() valueChange: EventEmitter<string> = new EventEmitter<string>();
  @ViewChild('inputcontainer', { read: ViewContainerRef, static: true }) inputContainer;

  control: AbstractControl;
  match: string;
  pattern: string;
  required: boolean;

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }

    this.match = '';
    this.pattern = this.viewModel.validators ? this.viewModel.validators.pattern : '';
    this.required = !!this.viewModel.validators && this.viewModel.validators.required === true;

    const value: string = this.getValue();

    this.createInput();
    this.control = this.createFormControl(value);

    if (!this.formGroup.get(this.viewModel.controlId)) {
      this.formGroup.addControl(this.viewModel.controlId, this.control);
    }
  }

  ngOnDestroy(): void {}

  validate(): void {
    this.control.updateValueAndValidity();
  }

  private bindValue(value: string): void {
    this.viewModel.value = value;

    this.valueChange.emit(this.viewModel.value);
    this.changeDetector.detectChanges();
  }

  private createFormControl(value: string): AbstractControl {
    const validators: ValidatorFn[] = [];

    if (this.required) {
      validators.push(Validators.required);
    }

    if (this.pattern) {
      validators.push(Validators.pattern(this.pattern));
    }


    let control: AbstractControl = this.formGroup.get(this.viewModel.controlId);
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

  private createInput(): void {
    this.inputContainer.clear();

    const factory: ComponentFactory<InputBase> = this.resolver.resolveComponentFactory(this.viewModel.componentFactory);
    const componentRef: ComponentRef<InputBase> = this.inputContainer.createComponent(factory);

    const instance: InputBase = componentRef.instance;
    instance.controlId = this.viewModel.controlId;
    instance.formGroup = this.formGroup;
  }

  private getValue(): string {
    return this.viewModel.value;
  }

  private onValueChanged(value: string): void {
    this.bindValue(value);
  }
}
