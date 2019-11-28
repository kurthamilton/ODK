import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';

import { FormComponent } from './components/forms/form/form.component';
import { FormControlsComponent } from './components/forms/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/forms/form-control-validation/form-control-validation.component';
import { FormControlComponent } from './components/forms/form-control/form-control.component';
import { ErrorMessagesComponent } from './components/elements/error-messages/error-messages.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { AppStyleModule } from './modules/app-style.module';

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
    AppStyleModule,
  ],
  exports: [
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
    AppStyleModule
  ],
})
export class SharedModule { }
