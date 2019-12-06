import { Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormStateViewModel } from '../form-state.view-model';
import { FormViewModel } from '../form.view-model';

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

  messages: string[];
  state: FormStateViewModel = {};
  submitting = false;

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

  ngOnDestroy(): void {}

  onSubmit(): void {
    this.state.validated = true;
    this.formSubmit.next();

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
