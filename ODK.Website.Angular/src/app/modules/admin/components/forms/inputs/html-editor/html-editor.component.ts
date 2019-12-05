import { Component, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import * as ClassicEditor from '@ckeditor/ckeditor5-build-classic';

import { InputBase } from 'src/app/modules/forms/components/inputs/input-base';

@Component({
  selector: 'app-html-editor',
  templateUrl: './html-editor.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HtmlEditorComponent extends InputBase {

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  Editor = ClassicEditor;
}
