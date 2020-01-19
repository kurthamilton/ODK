import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgSelectModule } from '@ng-select/ng-select';

import { AdminLayoutComponent } from './components/structure/admin-layout/admin-layout.component';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChapterAdminLayoutComponent } from './components/chapters/chapter-admin-layout/chapter-admin-layout.component';
import { ChapterAdminMemberAddComponent } from './components/chapters/chapter-admin-member-add/chapter-admin-member-add.component';
import { ChapterAdminMemberComponent } from './components/chapters/chapter-admin-member/chapter-admin-member.component';
import { ChapterAdminMembersComponent } from './components/chapters/chapter-admin-members/chapter-admin-members.component';
import { ChapterEmailProviderComponent } from './components/emails/chapter-email-provider/chapter-email-provider.component';
import { ChapterEmailProviderFormComponent } from './components/emails/chapter-email-provider-form/chapter-email-provider-form.component';
import { ChapterEmailProvidersComponent } from './components/emails/chapter-email-providers/chapter-email-providers.component';
import { ChapterEmailsComponent } from './components/emails/chapter-emails/chapter-emails.component';
import { ChapterMembershipSettingsComponent } from './components/chapters/chapter-membership-settings/chapter-membership-settings.component';
import { ChapterMenuComponent } from './components/chapters/chapter-menu/chapter-menu.component';
import { ChapterPaymentSettingsComponent } from './components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterPropertiesComponent } from './components/chapters/chapter-properties/chapter-properties.component';
import { ChapterPropertyComponent } from './components/chapters/chapter-property/chapter-property.component';
import { ChapterQuestionsComponent } from './components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from './components/chapters/chapter-settings/chapter-settings.component';
import { ChapterSubscriptionCreateComponent } from './components/chapters/chapter-subscription-create/chapter-subscription-create.component';
import { ChapterSubscriptionEditComponent } from './components/chapters/chapter-subscription-edit/chapter-subscription-edit.component';
import { ChapterSubscriptionFormComponent } from './components/chapters/chapter-subscription-form/chapter-subscription-form.component';
import { ChapterSubscriptionsComponent } from './components/chapters/chapter-subscriptions/chapter-subscriptions.component';
import { CreateChapterEmailProviderComponent } from './components/emails/create-chapter-email-provider/create-chapter-email-provider.component';
import { CreateEventComponent } from './components/events/create-event/create-event.component';
import { CreateVenueComponent } from './components/venues/create-venue/create-venue.component';
import { DefaultEmailsComponent } from './components/emails/default-emails/default-emails.component';
import { DropDownMultiFormControlComponent } from './components/forms/inputs/drop-down-multi-form-control/drop-down-multi-form-control.component';
import { EditEventComponent } from './components/events/edit-event/edit-event.component';
import { EditMemberImageComponent } from './components/members/edit-member-image/edit-member-image.component';
import { EditVenueComponent } from './components/venues/edit-venue/edit-venue.component';
import { EmailLayoutComponent } from './components/emails/email-layout/email-layout.component';
import { EmailMenuComponent } from './components/emails/email-menu/email-menu.component';
import { EventAttendeesComponent } from './components/events/event-attendees/event-attendees.component';
import { EventFormComponent } from './components/events/event-form/event-form.component';
import { EventHeaderComponent } from './components/events/event-header/event-header.component';
import { EventInviteeEmailComponent } from './components/events/event-invitee-email/event-invitee-email.component';
import { EventInvitesComponent } from './components/events/event-invites/event-invites.component';
import { EventLayoutComponent } from './components/events/event-layout/event-layout.component';
import { EventMenuComponent } from './components/events/event-menu/event-menu.component';
import { EventResponsesComponent } from './components/events/event-responses/event-responses.component';
import { EventsComponent } from './components/events/events/events.component';
import { GoogleMapsTextInputFormControlComponent } from './components/forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.component';
import { HtmlEditorFormControlComponent } from './components/forms/inputs/html-editor-form-control/html-editor-form-control.component';
import { LogComponent } from './components/admin/log/log.component';
import { MemberEventsComponent } from './components/members/member-events/member-events.component';
import { MemberFilterComponent } from './components/members/member-filter/member-filter.component';
import { MemberLayoutComponent } from './components/members/member-layout/member-layout.component';
import { MemberMenuComponent } from './components/members/member-menu/member-menu.component';
import { MembersComponent } from './components/members/members/members.component';
import { MemberSubscriptionComponent } from './components/members/member-subscription/member-subscription.component';
import { NavTabsComponent } from './components/elements/nav-tabs/nav-tabs.component';
import { PaginationComponent } from './components/elements/pagination/pagination.component';
import { SendEmailComponent } from './components/members/send-email/send-email.component';
import { SortableHeaderDirective } from './directives/sortable-header/sortable-header.directive';
import { SortableTableDirective } from './directives/sortable-table/sortable-table.directive';
import { SortButtonComponent } from './components/elements/sort-button/sort-button.component';
import { VenueEventsComponent } from './components/venues/venue-events/venue-events.component';
import { VenueFormComponent } from './components/venues/venue-form/venue-form.component';
import { VenueLayoutComponent } from './components/venues/venue-layout/venue-layout.component';
import { VenueMenuComponent } from './components/venues/venue-menu/venue-menu.component';
import { VenuesComponent } from './components/venues/venues/venues.component';
import { ChapterPropertyFormComponent } from './components/chapters/chapter-property-form/chapter-property-form.component';

@NgModule({
  declarations: [
    AdminLayoutComponent,
    AdminMenuComponent,
    ChapterAdminLayoutComponent,
    ChapterAdminMemberAddComponent,
    ChapterAdminMemberComponent,
    ChapterAdminMembersComponent,
    ChapterEmailProviderComponent,
    ChapterEmailProviderFormComponent,
    ChapterEmailProvidersComponent,
    ChapterEmailsComponent,
    ChapterMembershipSettingsComponent,
    ChapterMenuComponent,
    ChapterPaymentSettingsComponent,
    ChapterPropertiesComponent,
    ChapterPropertyComponent,
    ChapterQuestionsComponent,
    ChapterSettingsComponent,
    ChapterSubscriptionCreateComponent,
    ChapterSubscriptionEditComponent,
    ChapterSubscriptionFormComponent,
    ChapterSubscriptionsComponent,
    CreateChapterEmailProviderComponent,
    CreateEventComponent,
    CreateVenueComponent,
    DefaultEmailsComponent,
    DropDownMultiFormControlComponent,
    EditEventComponent,
    EditMemberImageComponent,
    EditVenueComponent,
    EmailLayoutComponent,
    EmailMenuComponent,
    EventAttendeesComponent,
    EventFormComponent,
    EventHeaderComponent,
    EventInviteeEmailComponent,
    EventInvitesComponent,
    EventLayoutComponent,
    EventMenuComponent,
    EventResponsesComponent,
    EventsComponent,
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent,
    LogComponent,
    MemberEventsComponent,
    MemberFilterComponent,
    MemberLayoutComponent,
    MemberMenuComponent,
    MembersComponent,
    MemberSubscriptionComponent,
    NavTabsComponent,
    PaginationComponent,
    SendEmailComponent,
    SortableHeaderDirective,
    SortableTableDirective,
    SortButtonComponent,
    VenueEventsComponent,
    VenueFormComponent,
    VenueLayoutComponent,
    VenueMenuComponent,
    VenuesComponent,
    ChapterPropertyFormComponent,
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
    DropDownMultiFormControlComponent,
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent
  ]
})
export class AdminModule { }
