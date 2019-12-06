import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppSharedModule } from '../shared/app-shared.module';
import { CheckBoxFormControlComponent } from './components/inputs/check-box-form-control/check-box-form-control.component';
import { DropDownFormControlComponent } from './components/inputs/drop-down-form-control/drop-down-form-control.component';
import { FormComponent } from './components/form/form.component';
import { ErrorMessagesComponent } from './components/error-messages/error-messages.component';
import { FormControlComponent } from './components/form-control/form-control.component';
import { FormControlLabelComponent } from './components/form-control-label/form-control-label.component';
import { FormControlsComponent } from './components/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/form-control-validation/form-control-validation.component';
import { ReadOnlyFormControlComponent } from './components/inputs/read-only-form-control/read-only-form-control.component';
import { TextAreaComponent } from './components/inputs/text-area-form-control/text-area-form-control.component';
import { TextInputFormControlComponent } from './components/inputs/text-input-form-control/text-input-form-control.component';

@NgModule({
  declarations: [
    CheckBoxFormControlComponent,
    DropDownFormControlComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    ErrorMessagesComponent,
    FormControlLabelComponent,
    FormControlValidationComponent,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputFormControlComponent,
  ],
  imports: [
    AppSharedModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    CheckBoxFormControlComponent,
    DropDownFormControlComponent,
    FormComponent,
    FormControlsComponent,
    ErrorMessagesComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputFormControlComponent,
  ],
  entryComponents: [
    CheckBoxFormControlComponent,
    DropDownFormControlComponent,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputFormControlComponent,
  ]
})
export class AppFormsModule { }
