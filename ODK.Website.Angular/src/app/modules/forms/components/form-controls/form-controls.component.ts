import { Component, OnChanges, ChangeDetectionStrategy, Input, ChangeDetectorRef, SimpleChanges, OnDestroy } from '@angular/core';
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

  constructor(private changeDetector: ChangeDetectorRef
  ) {
  }

  @Input() form: FormViewModel;
  @Input() validated: boolean;

  formGroup: FormGroup;
  updated: Subject<boolean> = new Subject<boolean>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['form']) {
      this.formGroup = this.createFormGroup();
      // initialise subject with initial status
      this.onGroupChanged(this.formGroup);
    }
  }

  ngOnDestroy(): void {
    this.updated.complete();
  }

  onValueChange(): void {
    this.form.valid = this.formGroup.valid;
  }

  private createFormGroup(): FormGroup {
    // close any open form group subscriptions
    this.updated.next();

    const group: FormGroup = new FormGroup({});

    group.valueChanges
      .pipe(
        takeUntil(this.updated),
        takeUntil(componentDestroyed(this)),
      )
      .subscribe(() => this.onGroupChanged(group));

    return group;
  }

  private onGroupChanged(group: FormGroup): void {
    this.form.valid = group.valid;
    this.changeDetector.detectChanges();
  }
}
