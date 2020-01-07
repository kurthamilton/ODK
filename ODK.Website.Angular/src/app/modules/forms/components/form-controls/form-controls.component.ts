import { Component, ChangeDetectionStrategy, OnChanges, Input, ViewChild, ViewContainerRef, ChangeDetectorRef, OnDestroy, SimpleChanges, EventEmitter, Output } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormStateViewModel } from '../form-state.view-model';
import { FormViewModel } from '../form/form.view-model';

@Component({
  selector: 'app-form-controls',
  templateUrl: './form-controls.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormControlsComponent implements OnChanges, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  @Input() form: FormViewModel;
  @Input() formSubmit: EventEmitter<void>;
  @Input() state: FormStateViewModel;
  @Input() validated: boolean;
  @Input() validateForm: Observable<void>;
  @Output() change: EventEmitter<void> = new EventEmitter<void>();

  @ViewChild('container', { read: ViewContainerRef, static: true }) container;

  formGroup: FormGroup;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['form']) {
      this.formGroup = this.createFormGroup();
      this.onFormGroupChanged(this.formGroup);
    }
  }

  ngOnDestroy(): void {}

  onValueChange(): void {
    this.onFormGroupChanged(this.formGroup);
  }

  private createFormGroup(): FormGroup {
    const formGroup: FormGroup = new FormGroup({});

    formGroup.valueChanges
      .pipe(
        takeUntil(componentDestroyed(this)),
      )
      .subscribe(() => {
        this.onFormGroupChanged(formGroup);
        this.changeDetector.detectChanges();
      });

    return formGroup;
  }

  private onFormGroupChanged(formGroup: FormGroup): void {
    this.state.valid = formGroup.valid;
    this.change.emit();
  }
}
