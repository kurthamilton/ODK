import { Component, ChangeDetectionStrategy, OnChanges, Input, ViewChild, ViewContainerRef, ChangeDetectorRef, OnDestroy, SimpleChanges } from '@angular/core';
import { FormGroup } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
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
  @Input() validated: boolean;

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
    this.form.valid = formGroup.valid;
  }
}
