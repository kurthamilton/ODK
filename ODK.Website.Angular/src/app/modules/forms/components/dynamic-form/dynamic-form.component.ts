import { Component, ChangeDetectionStrategy, Input } from '@angular/core';
import { DynamicFormViewModel } from '../dynamic-form.view-model';

@Component({
  selector: 'app-dynamic-form',
  templateUrl: './dynamic-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DynamicFormComponent {

  @Input() form: DynamicFormViewModel;

  messages: string[];
  submitting = false;

  onSubmit(): void {
    console.log('submit');
  }
}
