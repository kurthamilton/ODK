import { Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { Subject } from 'rxjs';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormStateViewModel } from '../form-state.view-model';
import { FormViewModel } from './form.view-model';
import { LoadingSpinnerOptions } from 'src/app/modules/shared/components/elements/loading-spinner/loading-spinner-options';

@Component({
  selector: 'app-form',
  templateUrl: './form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  @Input() form: FormViewModel;
  @Output() formSubmit: EventEmitter<void> = new EventEmitter<void>();

  loadingOptions: LoadingSpinnerOptions = {
    overlay: true
  };
  messages: string[];
  state: FormStateViewModel = {};
  submitting = false;
  validateForm: Subject<void> = new Subject<void>();

  ngOnInit(): void {
    if (!this.form.callback) {
      return;
    }

    this.form.callback.pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((result: boolean | string[]) => {
      let success: boolean;
      if (typeof(result) === 'boolean') {
        success = result;
      } else {
        this.messages = result;
        this.changeDetector.detectChanges();
      }

      this.onFormCallback(success);
    });
  }

  ngOnDestroy(): void {
    this.validateForm.complete();
  }

  onSubmit(): void {
    this.state.validated = true;
    this.validateForm.next();

    // form validity is set in form controls component
    if (!this.state.valid) {
      this.changeDetector.detectChanges();
      return;
    }

    this.submitting = true;
    this.changeDetector.detectChanges();

    this.formSubmit.emit();
  }

  private onFormCallback(success: boolean) {
    this.submitting = false;
    this.state.validated = !success;
    this.changeDetector.detectChanges();
  }
}
