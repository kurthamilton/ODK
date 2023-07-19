import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { InputGroupPrependViewModel } from './input-group-prepend.view-model';

@Component({
  selector: 'app-input-group-prepend',
  templateUrl: './input-group-prepend.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InputGroupPrependComponent {
  @Input() viewModel: InputGroupPrependViewModel;
}
