import { FormControlOptions } from '../../form-control-options';

export interface FileInputFormControlOptions extends FormControlOptions {
  fileType: 'image';
  value?: FileList;
}
