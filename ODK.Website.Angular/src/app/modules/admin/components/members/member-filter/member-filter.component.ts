import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter } from '@angular/core';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { FormControlLabelViewModel } from 'src/app/modules/forms/components/form-control-label.view-model';
import { MemberFilterViewModel } from './member-filter.view-model';
import { SubscriptionType } from 'src/app/core/account/subscription-type';

@Component({
  selector: 'app-member-filter',
  templateUrl: './member-filter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberFilterComponent implements OnChanges {

  constructor() { }

  @Input() viewModel: MemberFilterViewModel;
  @Output() change: EventEmitter<MemberFilterViewModel> = new EventEmitter<MemberFilterViewModel>();

  typeLabel: FormControlLabelViewModel = {
    text: 'Type'
  };
  typeOptions: DropDownFormControlOption[];

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }
    
    this.buildForm();
  }

  onChange(): void {
    const viewModel: MemberFilterViewModel = {
      types: this.typeOptions.filter(x => x.selected).map(x => <SubscriptionType>parseInt(x.value, 10))
    };

    this.change.emit(viewModel);
  }
  
  onTypeChange(selectedOptions: DropDownFormControlOption[]): void {
    this.typeOptions.forEach(option => {
      option.selected = !!selectedOptions.find(x => x.value === option.value);
    });
    this.onChange();
  }

  private buildForm(): void {
    this.typeOptions = [
      this.createTypeOption(SubscriptionType.Trial),
      this.createTypeOption(SubscriptionType.Full),
      this.createTypeOption(SubscriptionType.Partial),
      this.createTypeOption(SubscriptionType.Alum)
    ];
  }

  private createTypeOption(type: SubscriptionType): DropDownFormControlOption {
    return { 
      text: SubscriptionType[type], 
      value: type.toString(), 
      selected: this.viewModel.types.includes(type)
    };
  }
}
