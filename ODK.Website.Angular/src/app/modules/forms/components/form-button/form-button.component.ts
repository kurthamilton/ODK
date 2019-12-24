import { Component, ChangeDetectionStrategy, Input } from '@angular/core';

import { FormButtonViewModel } from './form-button.view-model';

@Component({
  selector: 'app-form-button',
  templateUrl: './form-button.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FormButtonComponent {

  constructor() { }

  @Input() button: FormButtonViewModel;
  
}
