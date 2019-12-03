import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { DynamicFormControlLabelViewModel } from '../dynamic-form-control-label.view-model';

@Component({
  selector: 'app-dynamic-form-label',
  templateUrl: './dynamic-form-label.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicFormLabelComponent {

  @Input() viewModel: DynamicFormControlLabelViewModel;

}
