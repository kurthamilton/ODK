import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { FileInputFormControlViewModel } from './file-input-form-control.view-model';
import { InputBase } from '../input-base';

@Component({
  selector: 'app-file-input-form-control',
  templateUrl: './file-input-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FileInputFormControlComponent extends InputBase implements OnDestroy {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  get accept(): string { return (this.viewModel as FileInputFormControlViewModel).accept; }

  private files: FileList;

  ngOnDestroy(): void {}

  onFileChange(files: FileList): void {
    this.files = files;
    if (!this.control.value) {
      // hack - some browsers do not set the file input value
      this.control.setValue(files);
    }
    this.onValidate();
  }

  protected setValue(_: any): void {
    this.viewModel.value = this.files;
  }
}
