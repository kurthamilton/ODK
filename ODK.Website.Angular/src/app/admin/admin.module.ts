import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './routing/admin-routing.module';
import { AdminComponent } from './components/admin/admin.component';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterLayoutComponent } from './components/layouts/chapter-layout/chapter-layout.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { HomeComponent } from './components/home/home/home.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { LoginComponent } from './components/account/login/login.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { AdminChapterMenuComponent } from './components/structure/chapter-menu/admin-chapter-menu.component';
import { EventsComponent } from './components/events/events/events.component';
import { EventComponent } from './components/events/event/event.component';
import { SharedModule } from '../shared.module';


@NgModule({
  declarations: [
    AdminComponent,
    ChapterComponent,
    ChapterLayoutComponent,
    AdminChapterMenuComponent,
    CreateEventComponent,
    EventsComponent,
    HomeComponent,
    EventComponent,
    EventFormComponent,
    LoginComponent,
    EditEventComponent,
  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    SharedModule
  ]
})
export class AdminModule { }
