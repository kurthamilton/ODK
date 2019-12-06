import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';

import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';
import { takeUntil } from 'rxjs/operators';
import { componentDestroyed } from 'src/app/rxjs/component-destroyed';

@Component({
  selector: 'app-html-editor-form-control',
  templateUrl: './html-editor-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HtmlEditorFormControlComponent extends InputBase implements OnInit, OnDestroy {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  Editor = ClassicEditor;

  isInvalid = false;
  isValid = false;

  private submitted = false;

  ngOnInit(): void {
    this.formSubmit.pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe(() => {
      this.submitted = true;
      this.validate();
      this.changeDetector.detectChanges();
    });
  }

  ngOnDestroy(): void {}

  onChange() {
    this.control.markAsTouched();
    super.onValidate();
    this.validate();
    this.changeDetector.detectChanges();
  }

  private validate(): void {
    if (!this.submitted) {
      return;
    }

    this.isInvalid = this.control.invalid;
    this.isValid = this.control.valid;
  }
}
