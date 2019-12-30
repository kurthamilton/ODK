import { Component, ChangeDetectionStrategy, ChangeDetectorRef, OnDestroy } from '@angular/core';

import { takeUntil } from 'rxjs/operators';

import { componentDestroyed } from 'src/app/rxjs/component-destroyed';
import { DropDownFormControlOption } from './drop-down-form-control-option';
import { DropDownFormControlViewModel } from './drop-down-form-control.view-model';
import { InputBase } from '../input-base';
import { TextInputFormControlViewModel } from '../text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-drop-down-form-control',
  templateUrl: './drop-down-form-control.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DropDownFormControlComponent extends InputBase implements OnDestroy {

  private _options: DropDownFormControlOption[];

  constructor(changeDetector: ChangeDetectorRef) {
    super(changeDetector);
  }

  freeTextViewModel: TextInputFormControlViewModel;
  get options(): DropDownFormControlOption[] { return this._options; }

  private defaultOption: DropDownFormControlOption;
  private dropDownViewModel: DropDownFormControlViewModel;
  private freeText: string;
  private freeTextOption: DropDownFormControlOption;
  private selectedOption: DropDownFormControlOption;

  ngOnDestroy(): void {}

  onFreeTextValueChange(): void {
    this.freeText = this.freeTextViewModel.value;
    this.dropDownViewModel.setValue(this.selectedOption, this.freeText);
    this.valueChange.emit();
  }

  protected onInit(): void {
    this.dropDownViewModel = <DropDownFormControlViewModel>this.viewModel;

    this._options = this.dropDownViewModel.options;
    this.defaultOption = this._options.find(x => x.default);
    this.freeTextOption = this._options.find(x => x.freeText);

    const selectedOption: DropDownFormControlOption = this.getOption(this.viewModel.value);
    if (selectedOption.freeText) {
      this.freeText = this.viewModel.value;
    }

    this.control.setValue(selectedOption.value);
    
    this.control.valueChanges.pipe(
      takeUntil(componentDestroyed(this))
    ).subscribe((value: string) => {
      this.setSelectedOption(value);
      this.changeDetector.detectChanges();
    });

    this.setSelectedOption(selectedOption.value);
    this.changeDetector.detectChanges();
  }

  private getOption(value: string): DropDownFormControlOption {
    const selectedOption: DropDownFormControlOption = this._options.find(x => x.value === value);
    if (selectedOption) {
      return selectedOption;
    }

    if (value && this.freeTextOption) {
      return this.freeTextOption;
    }

    return this.defaultOption ? this.defaultOption : this.options[0];
  }

  private setSelectedOption(value: string): void {
    const selectedOption: DropDownFormControlOption = this.getOption(value);
    if (!selectedOption) {
      return;
    }

    this.options.forEach(x => x.selected = false);
    selectedOption.selected = true;
    this.selectedOption = selectedOption;

    if (selectedOption.freeText) {
      this.freeTextViewModel = new TextInputFormControlViewModel({
        id: `${this.viewModel.id}-freetext`,
        label: {
          text: ''
        },
        validation: this.viewModel.validation,
        value: this.freeText
      });

      this.onFreeTextValueChange();
    } else {
      this.freeTextViewModel = null;
    }

    this.dropDownViewModel.setValue(selectedOption, this.freeText);
  }
}
