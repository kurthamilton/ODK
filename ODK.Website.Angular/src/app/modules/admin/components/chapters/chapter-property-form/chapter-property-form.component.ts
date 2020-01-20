import { Component, ChangeDetectionStrategy, OnChanges, Input, Output, EventEmitter } from '@angular/core';

import { Subject } from 'rxjs';

import { ChapterProperty } from 'src/app/core/chapters/chapter-property';
import { CheckBoxFormControlViewModel } from 'src/app/modules/forms/components/inputs/check-box-form-control/check-box-form-control.view-model';
import { DataType } from 'src/app/core/data-types/data-type';
import { DropDownFormControlViewModel } from 'src/app/modules/forms/components/inputs/drop-down-form-control/drop-down-form-control.view-model';
import { FormViewModel } from 'src/app/modules/forms/components/form/form.view-model';
import { TextInputFormControlViewModel } from 'src/app/modules/forms/components/inputs/text-input-form-control/text-input-form-control.view-model';

@Component({
  selector: 'app-chapter-property-form',
  templateUrl: './chapter-property-form.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChapterPropertyFormComponent implements OnChanges {

  constructor() { }

  @Input() formCallback: Subject<boolean | string[]>;
  @Input() property: ChapterProperty;
  @Output() formSubmit: EventEmitter<ChapterProperty> = new EventEmitter<ChapterProperty>();

  form: FormViewModel;

  private formControls: {
    dataType: DropDownFormControlViewModel;
    helpText: TextInputFormControlViewModel;    
    hidden: CheckBoxFormControlViewModel;
    label: TextInputFormControlViewModel;
    name: TextInputFormControlViewModel;    
    required: CheckBoxFormControlViewModel;
    subtitle: TextInputFormControlViewModel;
  };
  
  ngOnChanges(): void {
    if (!this.property) {
      return;
    }
    
    this.buildForm();
  }

  onFormSubmit(): void {
    this.property.dataType = parseInt(this.formControls.dataType.value);
    this.property.helpText = this.formControls.helpText.value;
    this.property.hidden = this.formControls.hidden.value;
    this.property.label = this.formControls.label.value;
    this.property.name = this.formControls.name.value;
    this.property.required = this.formControls.required.value;
    this.property.subtitle = this.formControls.subtitle.value;

    this.formSubmit.emit(this.property);
  }

  private buildForm(): void {
    this.formControls = {
      dataType: new DropDownFormControlViewModel({
        id: 'data-type',
        label: {
          text: 'Data type'
        },
        options: [
          { text: 'Text', value: DataType.Text.toString() },
          { text: 'Long text', value: DataType.LongText.toString() },
          { text: 'Url', value: DataType.Url.toString() },
          { text: 'Drop down', value: DataType.DropDown.toString() }
        ],
        validation: {
          required: true
        },
        value: this.property ? this.property.dataType.toString() : DataType.None.toString()
      }),
      helpText: new TextInputFormControlViewModel({
        id: 'help-text',
        label: {
          helpText: 'This is how help text is displayed',
          text: 'Help text'
        },
        value: this.property ? this.property.helpText : ''
      }),
      hidden: new CheckBoxFormControlViewModel({
        id: 'hidden',
        label: {
          helpText: 'Hides this property from signup and profile forms',
          text: 'Hidden'
        },
        value: this.property ? this.property.hidden : false
      }),
      label: new TextInputFormControlViewModel({
        id: 'label',
        label: {
          text: 'Label'
        },
        validation: {
          required: true
        },
        value: this.property ? this.property.label : ''
      }),
      name: new TextInputFormControlViewModel({
        id: 'name',
        label: {
          helpText: 'How this property is referred to in new member emails',
          text: 'Name'
        },
        validation: {
          message: 'Name should only contain letters',
          pattern: '^\\w+$',
          required: true
        },
        value: this.property ? this.property.name : ''
      }),
      required: new CheckBoxFormControlViewModel({
        id: 'required',
        label: {
          text: 'Required'
        },
        value: this.property.required
      }),
      subtitle: new TextInputFormControlViewModel({
        id: 'subtitle',
        label: {
          subtitle: 'This is how a subtitle is displayed',
          text: 'Subtitle'
        },
        value: this.property ? this.property.subtitle : ''
      })
    };

    this.form = {
      buttons: [
        { text: 'Update' }
      ],
      callback: this.formCallback,
      controls: [
        this.formControls.hidden,
        this.formControls.name,
        this.formControls.label,
        this.formControls.dataType,
        this.formControls.required,
        this.formControls.helpText,
        this.formControls.subtitle
      ],
      messages: {
        success: this.property.id
          ? 'Property updated'
          : 'Property created'
      }
    };
  }
}
