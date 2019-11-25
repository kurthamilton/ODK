import { Component, OnInit, ChangeDetectionStrategy, Input, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { FormControl } from '@angular/forms';

import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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

  private destroyed: Subject<{}> = new Subject<{}>();

  ngOnInit(): void {
    this.control.statusChanges
      .pipe(takeUntil(this.destroyed))
      .subscribe(() => this.onStatusChanged());
  }

  ngOnDestroy(): void {
    this.destroyed.next({});
  }

  private onStatusChanged(): void {
    this.changeDetector.detectChanges();
  }
}
