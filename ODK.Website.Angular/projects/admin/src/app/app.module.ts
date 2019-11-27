import { DatePipe } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';

import { AppComponent } from './components/app/app.component';
import { AppRoutingModule } from './routing/app-routing.module';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterLayoutComponent } from './components/layouts/chapter-layout/chapter-layout.component';
import { ChapterMenuComponent } from './components/structure/chapter-menu/chapter-menu.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { EventsComponent } from './components/events/events/events.component';
import { HomeComponent } from './components/home/home/home.component';
import { AppBaseModule } from './modules/add-base.module';
import { EventComponent } from './components/events/event/event.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { LoginComponent } from './components/account/login/login.component';

@NgModule({
  declarations: [
    AppComponent,
    ChapterComponent,
    ChapterLayoutComponent,
    ChapterMenuComponent,
    CreateEventComponent,
    EventsComponent,
    HomeComponent,
    EventComponent,
    EventFormComponent,
    LoginComponent,
  ],
  imports: [
    AppBaseModule,
    AppRoutingModule,
    HttpClientModule
  ],
  providers: [    
    { provide: DatePipe, useClass: DatePipe }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
