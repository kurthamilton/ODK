import { Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { DynamicFormViewModel } from '../dynamic-form.view-model';
import { Subject } from 'rxjs';

@Component({
  selector: 'app-dynamic-form',
  templateUrl: './dynamic-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicFormComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  @Input() form: DynamicFormViewModel;
  @Output() formSubmit: EventEmitter<void> = new EventEmitter<void>();

  messages: string[];
  submitting = false;

  update: Subject<boolean> = new Subject<boolean>();
  
  ngOnInit(): void {
    if (!this.form.callback) {
      return;
    }

    this.form.callback.subscribe((result: boolean | string[]) => {
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
    this.update.complete();
  }

  onSubmit(): void {
    this.form.validated = true;

    // form validity is set in form controls component
    if (!this.form.valid) {
      this.changeDetector.detectChanges();
      return;
    }

    this.update.next(true);
    this.submitting = true;
    this.changeDetector.detectChanges();

    this.formSubmit.emit();
  }

  private onFormCallback(success: boolean) {
    this.update.next(false);
    this.submitting = false;
    this.form.validated = !success;
    this.changeDetector.detectChanges();
  }
}
