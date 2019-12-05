import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { FormControlLabelViewModel } from '../form-control-label.view-model';

@Component({
  selector: 'app-form-control-label',
  templateUrl: './form-control-label.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormControlLabelComponent {

  @Input() controlId: string;
  @Input() viewModel: FormControlLabelViewModel;

}
