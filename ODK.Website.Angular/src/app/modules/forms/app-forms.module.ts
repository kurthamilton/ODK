import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppSharedModule } from '../shared/app-shared.module';
import { ErrorMessagesComponent } from './components/error-messages/error-messages.component';
import { FormComponent } from './components/form/form.component';
import { FormControlComponent } from './components/form-control/form-control.component';
import { FormControlsComponent } from './components/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/form-control-validation/form-control-validation.component';
import { TextInputComponent } from './components/inputs/text-input/text-input.component';
import { DynamicFormControlsComponent } from './components/dynamic-form-controls/dynamic-form-controls.component';
import { DynamicFormComponent } from './components/dynamic-form/dynamic-form.component';
import { DynamicFormControlComponent } from './components/dynamic-form-control/dynamic-form-control.component';
import { FormControlLabelComponent } from './components/form-control-label/form-control-label.component';
import { CheckBoxComponent } from './components/inputs/check-box/check-box.component';
import { TextAreaComponent } from './components/inputs/text-area/text-area.component';
import { ReadOnlyFormControlComponent } from './components/inputs/read-only-form-control/read-only-form-control.component';

@NgModule({
  declarations: [
    DynamicFormControlsComponent,
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    TextInputComponent,
    DynamicFormComponent,
    DynamicFormControlComponent,
    FormControlLabelComponent,
    CheckBoxComponent,
    TextAreaComponent,
    ReadOnlyFormControlComponent,
  ],
  imports: [
    AppSharedModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    DynamicFormComponent,
    DynamicFormControlsComponent,
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
    TextInputComponent,
    TextAreaComponent,
    CheckBoxComponent,
    ReadOnlyFormControlComponent
  ],
  entryComponents: [
    TextInputComponent,
    CheckBoxComponent,
    TextAreaComponent,
    ReadOnlyFormControlComponent
  ]
})
export class AppFormsModule { }
