import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppStyleModule } from 'src/app/modules/app-style.module';
import { ErrorMessagesComponent } from 'src/app/components/elements/error-messages/error-messages.component';
import { FormComponent } from 'src/app/components/forms/form/form.component';
import { FormControlComponent } from 'src/app/components/forms/form-control/form-control.component';
import { FormControlsComponent } from 'src/app/components/forms/form-controls/form-controls.component';
import { FormControlValidationComponent } from 'src/app/components/forms/form-control-validation/form-control-validation.component';
import { HttpAuthInterceptorService } from 'src/app/services/http/http-auth-interceptor.service';

@NgModule({
    declarations: [        
        ErrorMessagesComponent,
        FormComponent,
        FormControlComponent,
        FormControlsComponent,
        FormControlValidationComponent,
    ],
    imports: [
        AppStyleModule,
        BrowserModule,
        FormsModule,
        ReactiveFormsModule
    ],
    exports: [
        AppStyleModule,
        BrowserModule,
        ErrorMessagesComponent,
        FormComponent,
        FormControlComponent,
        FormControlsComponent,
        FormControlValidationComponent,
    ],
    providers: [
        { provide: HTTP_INTERCEPTORS, useClass: HttpAuthInterceptorService, multi: true },
    ]
  })
  export class AppBaseModule { }
  