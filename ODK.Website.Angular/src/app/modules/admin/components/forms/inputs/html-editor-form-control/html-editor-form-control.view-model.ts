import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { HtmlEditorFormControlComponent } from './html-editor-form-control.component';
import { HtmlEditorFormControlOptions } from './html-editor-form-control-options';

export class HtmlEditorFormControlViewModel extends FormControlViewModel {

  constructor(options: HtmlEditorFormControlOptions) {
    super(options);
    this.value = options.value;
  }

  get type() { return HtmlEditorFormControlComponent; }
  value: string;
}
