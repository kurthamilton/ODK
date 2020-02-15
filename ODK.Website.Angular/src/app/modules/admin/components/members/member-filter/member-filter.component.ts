import { Component, ChangeDetectionStrategy, Input, OnChanges, Output, EventEmitter, OnDestroy } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { Subject } from 'rxjs';

import { DropDownFormControlOption } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control-option';
import { DropDownMultiFormControlViewModel } from '../../forms/inputs/drop-down-multi-form-control/drop-down-multi-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { MemberFilterViewModel } from './member-filter.view-model';
import { SubscriptionType } from 'src/app/core/account/subscription-type';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-member-filter',
  templateUrl: './member-filter.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MemberFilterComponent implements OnChanges, OnDestroy {

  constructor(private route: ActivatedRoute,
    private router: Router
  ) { 
  }

  @Input() viewModel: MemberFilterViewModel;
  @Output() change: EventEmitter<MemberFilterViewModel> = new EventEmitter<MemberFilterViewModel>();

  form: FormViewModel;

  private formCallback: Subject<boolean> = new Subject<boolean>();
  private formControls: {
    name: TextInputFormControlViewModel;
    types: DropDownMultiFormControlViewModel;
  };

  ngOnChanges(): void {
    if (!this.viewModel) {
      return;
    }
    
    this.buildForm();
  }

  ngOnDestroy(): void {
    this.formCallback.complete();
  }

  onFormChange(): void {
    const viewModel: MemberFilterViewModel = {
      name: this.formControls.name.value,
      types: this.formControls.types.value.map(x => <SubscriptionType>parseInt(x, 10))
    };

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        type: this.formControls.types.value.map(x => x)
      }
    })
    this.change.emit(viewModel);
  }

  onFormSubmit(): void {
    this.formCallback.next(true);
  }

  private buildForm(): void {
    this.formControls = {
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          text: 'Name'
        }        
      }),
      types: new DropDownMultiFormControlViewModel({
        id: 'types',
        label: {
          text: 'Types'
        },
        options: [
          this.createTypeOption(SubscriptionType.Trial),
          this.createTypeOption(SubscriptionType.Full),
          this.createTypeOption(SubscriptionType.Partial),
          this.createTypeOption(SubscriptionType.Alum)
        ],
        value: this.viewModel.types.map(x => x.toString())
      })
    };

    this.form = {
      buttons: [],
      callback: this.formCallback,
      controls: [
        this.formControls.types,
        this.formControls.name
      ],
      display: 'inline'
    };
  }

  private createTypeOption(type: SubscriptionType): DropDownFormControlOption {
    return { 
      text: SubscriptionType[type], 
      value: type.toString(), 
      selected: this.viewModel.types.includes(type)
    };
  }
}
