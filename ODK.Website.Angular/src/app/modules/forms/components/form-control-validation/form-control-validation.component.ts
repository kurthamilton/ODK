import { Component, OnInit, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';

import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';

@Component({
  selector: 'app-form-control-validation',
  templateUrl: './form-control-validation.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormControlValidationComponent implements OnInit, OnDestroy {

  constructor(private changeDetector: ChangeDetectorRef) {
  }

  @Input() control: FormControl;
  @Input() label: string;
  @Input() validated: boolean;

  ngOnInit(): void {
    this.control.statusChanges
      .pipe(takeUntil(componentDestroyed(this)))
      .subscribe(() => this.onStatusChanged());
  }

  ngOnDestroy(): void {}

  private onStatusChanged(): void {
    this.changeDetector.detectChanges();
  }
}
