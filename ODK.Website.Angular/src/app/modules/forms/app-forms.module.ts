import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppSharedModule } from '../shared/app-shared.module';
import { CheckBoxComponent } from './components/inputs/check-box/check-box.component';
import { DropDownFormControlComponent } from './components/inputs/drop-down-form-control/drop-down-form-control.component';
import { FormComponent } from './components/form/form.component';
import { ErrorMessagesComponent } from './components/error-messages/error-messages.component';
import { FormControlComponent } from './components/form-control/form-control.component';
import { FormControlLabelComponent } from './components/form-control-label/form-control-label.component';
import { FormControlsComponent } from './components/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/form-control-validation/form-control-validation.component';
import { ReadOnlyFormControlComponent } from './components/inputs/read-only-form-control/read-only-form-control.component';
import { TextAreaComponent } from './components/inputs/text-area/text-area.component';
import { TextInputComponent } from './components/inputs/text-input/text-input.component';

@NgModule({
  declarations: [
    CheckBoxComponent,
    DropDownFormControlComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    ErrorMessagesComponent,
    FormControlLabelComponent,
    FormControlValidationComponent,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputComponent,
  ],
  imports: [
    AppSharedModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    CheckBoxComponent,
    DropDownFormControlComponent,
    FormComponent,
    FormControlsComponent,
    ErrorMessagesComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputComponent,
  ],
  entryComponents: [
    CheckBoxComponent,
    DropDownFormControlComponent,
    ReadOnlyFormControlComponent,
    TextAreaComponent,
    TextInputComponent,
  ]
})
export class AppFormsModule { }
