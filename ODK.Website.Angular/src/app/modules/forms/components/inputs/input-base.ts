import { Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy, Directive } from '@angular/core';
import { FormGroup, AbstractControl } from '@angular/forms';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormControlViewModel } from '../form-control.view-model';
import { FormUtils } from '../../utils/form-utils';

@Directive()
export abstract class InputBase implements OnDestroy {

  private _viewModel: FormControlViewModel;

  protected constructor(protected changeDetector: ChangeDetectorRef,
    showLabel?: boolean
  ) {
    this.showLabel = showLabel !== false;
  }

  @Input() formGroup: FormGroup;
  @Input() validateForm: Observable<void>;
  @Input() set viewModel(value: FormControlViewModel) {
    if (this._viewModel) {
      return;
    }

    this._viewModel = value;
    this.controlId = value.id;
    if (value.validation) {
      this.pattern = value.validation.pattern;
      this.required = value.validation.required;
    }

    this.control = FormUtils.createControl(this.formGroup, value);
    this.init();
    this.changeDetector.detectChanges();
  }

  @Output() valueChange: EventEmitter<void> = new EventEmitter<void>();

  get viewModel(): FormControlViewModel {
    return this._viewModel;
  }

  control: AbstractControl;
  controlId: string;
  match: string;
  pattern: string;
  ready = false
  required: boolean;
  showLabel: boolean;
  
  ngOnDestroy(): void {}

  onValidate(): void {
    if (!this.control) {
      return;
    }
    
    this.control.updateValueAndValidity();
  }

  protected setValue(value: any): void {
    this.viewModel.value = value;
  }

  protected onInit(): void {
  }

  private init(): void {
    // bind value updates back to view models
    this.control.valueChanges
      .pipe(
        takeUntil(componentDestroyed(this))
      ).subscribe(x => this.onValueChanged(x));

    this.onInit();
  }

  private onValueChanged(value: string): void {
    this.setValue(value);
    this.valueChange.emit();
  }
}