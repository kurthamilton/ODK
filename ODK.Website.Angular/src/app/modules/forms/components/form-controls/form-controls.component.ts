import { Component, ChangeDetectionStrategy, OnChanges, Input, ViewChild, ViewContainerRef, ChangeDetectorRef, OnDestroy, SimpleChanges, EventEmitter } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { Subject, Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { FormStateViewModel } from '../form-state.view-model';
import { FormViewModel } from '../form.view-model';

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

  @ViewChild('container', { read: ViewContainerRef, static: true }) container;

  formGroup: FormGroup;
  updated: Subject<boolean> = new Subject<boolean>();  

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['form']) {
      this.formGroup = this.createFormGroup();
      this.onFormGroupChanged(this.formGroup);
    }
  }

  ngOnDestroy(): void {
    this.updated.complete();
  }

  onValueChange(): void {
    this.onFormGroupChanged(this.formGroup);
  }

  private createFormGroup(): FormGroup {
    // close any open form group subscriptions
    this.updated.next();

    const formGroup: FormGroup = new FormGroup({});

    formGroup.valueChanges
      .pipe(
        takeUntil(this.updated),
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
  }
}
