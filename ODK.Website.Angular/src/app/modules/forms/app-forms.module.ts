import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AppSharedModule } from '../shared/app-shared.module';
import { ErrorMessagesComponent } from './components/error-messages/error-messages.component';
import { FormComponent } from './components/form/form.component';
import { FormControlComponent } from './components/form-control/form-control.component';
import { FormControlsComponent } from './components/form-controls/form-controls.component';
import { FormControlValidationComponent } from './components/form-control-validation/form-control-validation.component';

@NgModule({
  declarations: [
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
  ],
  imports: [
    AppSharedModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
  ],
  exports: [
    ErrorMessagesComponent,
    FormComponent,
    FormControlComponent,
    FormControlsComponent,
    FormControlValidationComponent,
    FormsModule,
    ReactiveFormsModule,
  ],
})
export class AppFormsModule { }
