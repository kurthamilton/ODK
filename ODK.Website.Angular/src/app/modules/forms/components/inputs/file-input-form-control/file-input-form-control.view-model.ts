import { FileInputFormControlComponent } from './file-input-form-control.component';
import { FileInputFormControlOptions } from './file-input-form-control-options';
import { FormControlViewModel } from '../../form-control.view-model';

export class FileInputFormControlViewModel extends FormControlViewModel {
  constructor(options: FileInputFormControlOptions) {
    super(options);
    if (options.fileType === 'image') {
      this.accept = 'image/*';
    }
    this.value = options.value;
  }

  accept: string;
  get type() { return FileInputFormControlComponent; }
  value: FileList;
}