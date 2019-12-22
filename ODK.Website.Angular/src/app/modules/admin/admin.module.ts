import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgSelectModule } from '@ng-select/ng-select';

import { AdminComponent } from './components/admin/admin.component';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChapterComponent } from './components/chapters/chapter/chapter.component';
import { ChapterEmailsComponent } from './components/chapters/chapter-emails/chapter-emails.component';
import { ChapterPaymentSettingsComponent } from './components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterQuestionsComponent } from './components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from './components/chapters/chapter-settings/chapter-settings.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { CreateVenueComponent } from './components/venues/create-venue/create-venue.component';
import { DropDownMultiComponent } from './components/forms/inputs/drop-down-multi/drop-down-multi.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { EditVenueComponent } from './components/venues/edit-venue/edit-venue.component';
import { EventComponent } from './components/events/event/event.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { EventInvitesComponent } from './components/events/event-invites/event-invites.component';
import { EventResponsesComponent } from './components/events/event-responses/event-responses.component';
import { EventsComponent } from './components/events/events/events.component';
import { GoogleMapsTextInputFormControlComponent } from './components/forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.component';
import { HtmlEditorFormControlComponent } from './components/forms/inputs/html-editor-form-control/html-editor-form-control.component';
import { MemberComponent } from './components/members/member/member.component';
import { MemberFilterComponent } from './components/members/member-filter/member-filter.component';
import { MembersComponent } from './components/members/members/members.component';
import { MemberSubscriptionComponent } from './components/members/member-subscription/member-subscription.component';
import { PaginationComponent } from './components/elements/pagination/pagination.component';
import { VenueComponent } from './components/venues/venue/venue.component';
import { VenueFormComponent } from './components/venues/venue-form/venue-form.component';
import { VenuesComponent } from './components/venues/venues/venues.component';
import { VenueEventsComponent } from './components/venues/venue-events/venue-events.component';

@NgModule({
  declarations: [
    AdminComponent,
    AdminMenuComponent,
    ChapterComponent,
    ChapterPaymentSettingsComponent,
    ChapterSettingsComponent,
    CreateEventComponent,
    EditEventComponent,
    EventComponent,
    EventFormComponent,
    EventInvitesComponent,
    EventResponsesComponent,
    EventsComponent,
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent,
    MemberComponent,
    MembersComponent,
    VenuesComponent,
    CreateVenueComponent,
    VenueComponent,
    VenueFormComponent,
    EditVenueComponent,
    ChapterQuestionsComponent,
    ChapterEmailsComponent,
    MemberFilterComponent,
    DropDownMultiComponent,
    PaginationComponent,
    MemberSubscriptionComponent,
    VenueEventsComponent,
  ],
  imports: [
    AdminRoutingModule,
    AppFormsModule,
    AppSharedModule,
    CKEditorModule,
    CommonModule,
    NgSelectModule,
  ],
  entryComponents: [
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent
  ]
})
export class AdminModule { }
