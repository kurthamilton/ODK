import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ClipboardModule } from 'ngx-clipboard';

import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgSelectModule } from '@ng-select/ng-select';

import { AdminBodyComponent } from './components/structure/admin-body/admin-body.component';
import { AdminLayoutComponent } from './components/structure/admin-layout/admin-layout.component';
import { AdminMemberAddComponent } from './components/members/admin-member-add/admin-member-add.component';
import { AdminMemberComponent } from './components/members/admin-member/admin-member.component';
import { AdminMembersComponent } from './components/members/admin-members/admin-members.component';
import { AdminMenuComponent } from './components/structure/admin-menu/admin-menu.component';
import { AdminRoutingModule } from './routing/admin-routing.module';
import { AdminSubMenuComponent } from './components/structure/admin-sub-menu/admin-sub-menu.component';
import { AppFormsModule } from '../forms/app-forms.module';
import { AppSharedModule } from '../shared/app-shared.module';
import { ChapterAdminLayoutComponent } from './components/chapters/chapter-admin-layout/chapter-admin-layout.component';
import { ChapterEmailProviderComponent } from './components/emails/chapter-email-provider/chapter-email-provider.component';
import { ChapterEmailProviderFormComponent } from './components/emails/chapter-email-provider-form/chapter-email-provider-form.component';
import { ChapterEmailProvidersComponent } from './components/emails/chapter-email-providers/chapter-email-providers.component';
import { ChapterEmailsComponent } from './components/emails/chapter-emails/chapter-emails.component';
import { ChapterPaymentSettingsComponent } from './components/chapters/chapter-payment-settings/chapter-payment-settings.component';
import { ChapterPropertiesComponent } from './components/chapters/chapter-properties/chapter-properties.component';
import { ChapterPropertyComponent } from './components/chapters/chapter-property/chapter-property.component';
import { ChapterPropertyCreateComponent } from './components/chapters/chapter-property-create/chapter-property-create.component';
import { ChapterPropertyFormComponent } from './components/chapters/chapter-property-form/chapter-property-form.component';
import { ChapterQuestionComponent } from './components/chapters/chapter-question/chapter-question.component';
import { ChapterQuestionCreateComponent } from './components/chapters/chapter-question-create/chapter-question-create.component';
import { ChapterQuestionFormComponent } from './components/chapters/chapter-question-form/chapter-question-form.component';
import { ChapterQuestionsComponent } from './components/chapters/chapter-questions/chapter-questions.component';
import { ChapterSettingsComponent } from './components/chapters/chapter-settings/chapter-settings.component';
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
import { EventsAdminLayoutComponent } from './components/events/events-admin-layout/events-admin-layout.component';
import { EventsComponent } from './components/events/events/events.component';
import { GoogleMapsTextInputFormControlComponent } from './components/forms/inputs/google-maps-text-input-form-control/google-maps-text-input-form-control.component';
import { HtmlEditorFormControlComponent } from './components/forms/inputs/html-editor-form-control/html-editor-form-control.component';
import { LogComponent } from './components/admin/log/log.component';
import { MediaFileComponent } from './components/media/media-file/media-file.component';
import { MediaFilesComponent } from './components/media/media-files/media-files.component';
import { MembersAdminLayoutComponent } from './components/members/members-admin-layout/members-admin-layout.component';
import { MemberEventsComponent } from './components/members/member-events/member-events.component';
import { MemberFilterComponent } from './components/members/member-filter/member-filter.component';
import { MemberLayoutComponent } from './components/members/member-layout/member-layout.component';
import { MemberMenuComponent } from './components/members/member-menu/member-menu.component';
import { MembersComponent } from './components/members/members/members.component';
import { MembershipSettingsComponent } from './components/subscriptions/membership-settings/membership-settings.component';
import { MemberSubscriptionComponent } from './components/members/member-subscription/member-subscription.component';
import { NavTabsComponent } from './components/elements/nav-tabs/nav-tabs.component';
import { PaginationComponent } from './components/elements/pagination/pagination.component';
import { SendEmailComponent } from './components/members/send-email/send-email.component';
import { SortableHeaderDirective } from './directives/sortable-header/sortable-header.directive';
import { SortableTableDirective } from './directives/sortable-table/sortable-table.directive';
import { SortButtonComponent } from './components/elements/sort-button/sort-button.component';
import { SubscriptionCreateComponent } from './components/subscriptions/subscription-create/subscription-create.component';
import { SubscriptionEditComponent } from './components/subscriptions/subscription-edit/subscription-edit.component';
import { SubscriptionFormComponent } from './components/subscriptions/subscription-form/subscription-form.component';
import { SubscriptionsComponent } from './components/subscriptions/subscriptions/subscriptions.component';
import { VenueEventsComponent } from './components/venues/venue-events/venue-events.component';
import { VenueFormComponent } from './components/venues/venue-form/venue-form.component';
import { VenueLayoutComponent } from './components/venues/venue-layout/venue-layout.component';
import { VenueMenuComponent } from './components/venues/venue-menu/venue-menu.component';
import { VenuesComponent } from './components/venues/venues/venues.component';

@NgModule({
  declarations: [
    AdminBodyComponent,
    AdminLayoutComponent,
    AdminMemberAddComponent,
    AdminMemberComponent,
    AdminMembersComponent,
    AdminMenuComponent,
    AdminSubMenuComponent,
    ChapterAdminLayoutComponent,
    ChapterEmailProviderComponent,
    ChapterEmailProviderFormComponent,
    ChapterEmailProvidersComponent,
    ChapterEmailsComponent,
    ChapterPaymentSettingsComponent,
    ChapterPropertiesComponent,
    ChapterPropertyComponent,
    ChapterPropertyCreateComponent,
    ChapterPropertyFormComponent,
    ChapterQuestionComponent,
    ChapterQuestionCreateComponent,
    ChapterQuestionFormComponent,
    ChapterQuestionsComponent,
    ChapterSettingsComponent,
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
    EventsAdminLayoutComponent,
    EventsComponent,
    GoogleMapsTextInputFormControlComponent,
    HtmlEditorFormControlComponent,
    LogComponent,
    MediaFileComponent,
    MediaFilesComponent,
    MembersAdminLayoutComponent,
    MemberEventsComponent,
    MemberFilterComponent,
    MemberLayoutComponent,
    MemberMenuComponent,
    MembersComponent,
    MembershipSettingsComponent,
    MemberSubscriptionComponent,
    NavTabsComponent,
    PaginationComponent,
    SendEmailComponent,
    SortableHeaderDirective,
    SortableTableDirective,
    SortButtonComponent,
    SubscriptionCreateComponent,
    SubscriptionEditComponent,
    SubscriptionFormComponent,
    SubscriptionsComponent,
    VenueEventsComponent,
    VenueFormComponent,
    VenueLayoutComponent,
    VenueMenuComponent,
    VenuesComponent,
  ],
  imports: [
    AdminRoutingModule,
    AppFormsModule,
    AppSharedModule,
    CKEditorModule,
    ClipboardModule,
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
