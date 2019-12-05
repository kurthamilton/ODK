import { Component, OnInit, ChangeDetectionStrategy, Input, Output, EventEmitter, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
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
  submitting = false;

  update: Subject<boolean> = new Subject<boolean>();

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
