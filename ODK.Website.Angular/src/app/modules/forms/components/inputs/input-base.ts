import { Input, OnInit, OnDestroy, ChangeDetectorRef, Output, EventEmitter } from '@angular/core';
import { AbstractControl, ValidatorFn, Validators, FormGroup, FormBuilder } from '@angular/forms';

import { Observable } from 'rxjs';

export abstract class InputBase {

  @Input() control: AbstractControl;
  @Input() controlId: string;
  @Input() formGroup: FormGroup;
  @Input() match: string;
  @Input() pattern: string;
  @Input() required: boolean;

  validate(): void {
    console.log('validate');
  }

  private onValueChanged(value: string): void {
    console.log('value changed', value);
  }
}