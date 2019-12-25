import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgSelectModule } from '@ng-select/ng-select';

import { AdminComponent } from './components/admin/admin.component';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChapterEmailsComponent } from './components/chapters/chapter-emails/chapter-emails.component';
import { ChapterAdminLayoutComponent } from './components/chapters/chapter-admin-layout/chapter-admin-layout.component';
import { ChapterMenuComponent } from './components/chapters/chapter-menu/chapter-menu.component';
import { ChapterPaymentSettingsComponent } from './components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterQuestionsComponent } from './components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from './components/chapters/chapter-settings/chapter-settings.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { CreateVenueComponent } from './components/venues/create-venue/create-venue.component';
import { DropDownMultiComponent } from './components/forms/inputs/drop-down-multi/drop-down-multi.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { EditMemberImageComponent } from './components/members/edit-member-image/edit-member-image.component';
import { EditVenueComponent } from './components/venues/edit-venue/edit-venue.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { EventHeaderComponent } from './components/events/event-header/event-header.component';
import { EventInvitesComponent } from './components/events/event-invites/event-invites.component';
import { EventLayoutComponent } from './components/events/event-layout/event-layout.component';
import { EventMenuComponent } from './components/events/event-menu/event-menu.component';
import { EventResponsesComponent } from './components/events/event-responses/event-responses.component';
import { EventsComponent } from './components/events/events/events.component';
import { GoogleMapsTextInputFormControlComponent } from './components/forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.component';
import { HtmlEditorFormControlComponent } from './components/forms/inputs/html-editor-form-control/html-editor-form-control.component';
import { MemberFilterComponent } from './components/members/member-filter/member-filter.component';
import { MemberLayoutComponent } from './components/members/member-layout/member-layout.component';
import { MemberMenuComponent } from './components/members/member-menu/member-menu.component';
import { MembersComponent } from './components/members/members/members.component';
import { MemberSubscriptionComponent } from './components/members/member-subscription/member-subscription.component';
import { NavTabsComponent } from './components/elements/nav-tabs/nav-tabs.component';
import { PaginationComponent } from './components/elements/pagination/pagination.component';
import { VenueEventsComponent } from './components/venues/venue-events/venue-events.component';
import { VenueFormComponent } from './components/venues/venue-form/venue-form.component';
import { VenueLayoutComponent } from './components/venues/venue-layout/venue-layout.component';
import { VenueMenuComponent } from './components/venues/venue-menu/venue-menu.component';
import { VenuesComponent } from './components/venues/venues/venues.component';
import { ChapterEmailProviderComponent } from './components/chapters/chapter-email-provider/chapter-email-provider.component';

@NgModule({
  declarations: [
    AdminComponent,
    AdminMenuComponent,
    ChapterEmailsComponent,
    ChapterAdminLayoutComponent,
    ChapterMenuComponent,
    ChapterPaymentSettingsComponent,
    ChapterQuestionsComponent,
    ChapterSettingsComponent,
    CreateEventComponent,
    CreateVenueComponent,
    DropDownMultiComponent,
    EditEventComponent,
    EditMemberImageComponent,
    EditVenueComponent,
    EventFormComponent,
    EventHeaderComponent,
    EventInvitesComponent,
    EventLayoutComponent,
    EventMenuComponent,
    EventResponsesComponent,
    EventsComponent,
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent,
    MemberFilterComponent,
    MemberLayoutComponent,
    MemberMenuComponent,
    MembersComponent,
    MemberSubscriptionComponent,
    NavTabsComponent,
    PaginationComponent,
    VenueEventsComponent,
    VenueFormComponent,
    VenueLayoutComponent,
    VenueMenuComponent,
    VenuesComponent,
    ChapterEmailProviderComponent,
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
