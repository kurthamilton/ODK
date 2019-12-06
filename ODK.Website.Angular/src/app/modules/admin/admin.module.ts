import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CKEditorModule } from '@ckeditor/ckeditor5-angular';

import { AdminComponent } from './components/admin/admin.component';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterPaymentSettingsComponent } from './components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { EventComponent } from './components/events/event/event.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { EventInvitesComponent } from './components/events/event-invites/event-invites.component';
import { EventResponsesComponent } from './components/events/event-responses/event-responses.component';
import { EventsComponent } from './components/events/events/events.component';
import { HtmlEditorComponent } from './components/forms/inputs/html-editor/html-editor.component';
import { MemberComponent } from './components/members/member/member.component';
import { MembersComponent } from './components/members/members/members.component';
import { GoogleMapsTextInputComponent } from './components/forms/inputs/google-maps-text-input/google-maps-text-input.component';

@NgModule({
  declarations: [
    AdminComponent,
    AdminMenuComponent,
    ChapterComponent,
    ChapterPaymentSettingsComponent,
    CreateEventComponent,
    EditEventComponent,
    EventComponent,
    EventFormComponent,
    EventInvitesComponent,
    EventResponsesComponent,
    EventsComponent,
    HtmlEditorComponent,
    MemberComponent,
    MembersComponent,
    GoogleMapsTextInputComponent,
  ],
  imports: [
    AdminRoutingModule,
    AppFormsModule,
    AppSharedModule,
    CKEditorModule,
    CommonModule,
  ],
  entryComponents: [
    GoogleMapsTextInputComponent,
    HtmlEditorComponent
  ]
})
export class AdminModule { }
