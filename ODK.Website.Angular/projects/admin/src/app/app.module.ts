import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppComponent } from './components/app/app.component';
import { AppRoutingModule } from './routing/app-routing.module';
import { HttpAuthInterceptorService } from 'src/app/services/http/http-auth-interceptor.service';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: HttpAuthInterceptorService, multi: true },
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
