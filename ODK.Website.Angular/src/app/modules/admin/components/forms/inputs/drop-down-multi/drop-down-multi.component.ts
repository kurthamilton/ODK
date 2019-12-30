import { Component, Input, ChangeDetectionStrategy, OnChanges, Output, EventEmitter } from '@angular/core';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { FormControlLabelViewModel } from 'src/app/modules/forms/components/form-control-label/form-control-label.view-model';

@Component({
  selector: 'app-drop-down-multi',
  templateUrl: './drop-down-multi.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropDownMultiComponent implements OnChanges {

  constructor() { }

  @Input() controlId: string;
  @Input() label: FormControlLabelViewModel;
  @Input() options: DropDownFormControlOption[];
  @Output() change: EventEmitter<DropDownFormControlOption[]> = new EventEmitter<DropDownFormControlOption[]>();

  selectedOptions: string[];

  ngOnChanges(): void {
    if (!this.options) {
      return;
    }

    this.selectedOptions = this.options.filter(x => x.selected).map(x => x.value);
  }

  onChange(): void {
    this.change.emit(this.options.filter(x => this.selectedOptions.includes(x.value)));
  }
}
