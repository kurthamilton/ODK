import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { ErrorMessagesComponent } from '../components/elements/error-messages/error-messages.component';
import { FormComponent } from '../components/forms/form/form.component';
import { FormControlsComponent } from '../components/forms/form-controls/form-controls.component';
import { FormControlValidationComponent } from '../components/forms/form-control-validation/form-control-validation.component';
import { FormControlComponent } from '../components/forms/form-control/form-control.component';
import { StyleModule } from './style.module';

@NgModule({
  declarations: [
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    StyleModule
  ],
  exports: [
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
    StyleModule
  ]
})
export class ElementsModule { }
