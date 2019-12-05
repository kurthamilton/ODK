import { FormControlViewModel } from 'src/app/modules/forms/components/form-control.view-model';
import { HtmlEditorComponent } from './html-editor.component';
import { HtmlEditorOptions } from './html-editor-options';

export class HtmlEditorViewModel extends FormControlViewModel {

  constructor(options: HtmlEditorOptions) {
    super(options);
    this.value = options.value;
  }

  get type() { return HtmlEditorComponent; }
  value: string;
}