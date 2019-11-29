import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminComponent } from './components/admin/admin.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { EventsComponent } from './components/events/events/events.component';
import { EventComponent } from './components/events/event/event.component';
import { SharedModule } from '../modules/shared.module';
import { EventInvitesComponent } from './components/events/event-invites/event-invites.component';
import { EventResponsesComponent } from './components/events/event-responses/event-responses.component';


@NgModule({
  declarations: [
    AdminComponent,
    AdminMenuComponent,
    CreateEventComponent,
    EditEventComponent,
    EventsComponent,
    EventComponent,
    EventFormComponent,
    EventInvitesComponent,
    EventResponsesComponent,
  ],
  imports: [
    AdminRoutingModule,
    CommonModule,
    SharedModule
  ]
})
export class AdminModule { }
